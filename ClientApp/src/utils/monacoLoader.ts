import editorWorker from 'monaco-editor/esm/vs/editor/editor.worker?worker';
import jsonWorker from 'monaco-editor/esm/vs/language/json/json.worker?worker';
import cssWorker from 'monaco-editor/esm/vs/language/css/css.worker?worker';
import htmlWorker from 'monaco-editor/esm/vs/language/html/html.worker?worker';
import tsWorker from 'monaco-editor/esm/vs/language/typescript/ts.worker?worker';

export type MonacoModule = typeof import('monaco-editor/esm/vs/editor/editor.api');

const featureAliases: Record<string, string> = {
	json: 'json',
	css: 'css',
	scss: 'css',
	less: 'css',
	html: 'html',
	handlebars: 'html',
	razor: 'html',
	typescript: 'typescript',
	javascript: 'typescript',
	go: 'go',
};

const featureImporters: Record<string, () => Promise<unknown>> = {
	json: () => import('monaco-editor/esm/vs/language/json/monaco.contribution'),
	css: () => import('monaco-editor/esm/vs/language/css/monaco.contribution'),
	html: () => import('monaco-editor/esm/vs/language/html/monaco.contribution'),
	typescript: () => import('monaco-editor/esm/vs/language/typescript/monaco.contribution'),
	go: () => import('monaco-editor/esm/vs/basic-languages/go/go.contribution'),
};

const featurePromises = new Map<string, Promise<unknown>>();
let monacoPromise: Promise<MonacoModule> | undefined;
let monacoEnvironmentConfigured = false;
let typescriptDefaultsConfigured = false;

const getMonacoModule = () => {
	monacoPromise ??= import('monaco-editor/esm/vs/editor/editor.api');
	return monacoPromise;
};

const getFeatureKeys = (languages: string | string[] = []) => {
	const normalizedLanguages = Array.isArray(languages) ? languages : [languages];
	return [...new Set(normalizedLanguages.map((language) => featureAliases[language?.toLowerCase()]).filter(Boolean))];
};

const loadFeature = (feature: string) => {
	const existingPromise = featurePromises.get(feature);
	if (existingPromise) return existingPromise;

	const importFeature = featureImporters[feature];
	if (!importFeature) return Promise.resolve();

	const pendingPromise = importFeature();
	featurePromises.set(feature, pendingPromise);
	return pendingPromise;
};

const ensureMonacoEnvironment = () => {
	if (monacoEnvironmentConfigured) return;

	const globalScope = self as typeof self & {
		MonacoEnvironment?: {
			getWorker: (_: string, label: string) => Worker;
		};
	};

	globalScope.MonacoEnvironment = {
		getWorker(_, label) {
			if (label === 'json') {
				return new jsonWorker();
			}
			if (label === 'css' || label === 'scss' || label === 'less') {
				return new cssWorker();
			}
			if (label === 'html' || label === 'handlebars' || label === 'razor') {
				return new htmlWorker();
			}
			if (label === 'typescript' || label === 'javascript') {
				return new tsWorker();
			}
			return new editorWorker();
		},
	};

	monacoEnvironmentConfigured = true;
};

const configureTypeScriptDefaults = (monaco: MonacoModule) => {
	if (typescriptDefaultsConfigured) return;

	monaco.languages.typescript.javascriptDefaults.setDiagnosticsOptions({
		noSemanticValidation: true,
		noSyntaxValidation: false,
	});
	monaco.languages.typescript.javascriptDefaults.setCompilerOptions({
		target: monaco.languages.typescript.ScriptTarget.ES2016,
		allowNonTsExtensions: true,
	});

	typescriptDefaultsConfigured = true;
};

export const loadMonacoEditor = async (languages: string | string[] = []) => {
	ensureMonacoEnvironment();

	const featureKeys = getFeatureKeys(languages);
	const [monaco] = await Promise.all([getMonacoModule(), ...featureKeys.map(loadFeature)]);

	if (featureKeys.includes('typescript')) {
		configureTypeScriptDefaults(monaco);
	}

	return monaco;
};
