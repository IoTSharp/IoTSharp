<template>
  <div  ref="container" :style="{ height: height, width: width }"></div>
</template>
<script lang="ts">
import {
  defineComponent,
  reactive,
  toRefs,
  ref,
  nextTick,
  getCurrentInstance,
} from "vue";

import * as monaco from "monaco-editor";
import editorWorker from "monaco-editor/esm/vs/editor/editor.worker?worker";
import jsonWorker from "monaco-editor/esm/vs/language/json/json.worker?worker";
import cssWorker from "monaco-editor/esm/vs/language/css/css.worker?worker";
import htmlWorker from "monaco-editor/esm/vs/language/html/html.worker?worker";
import tsWorker from "monaco-editor/esm/vs/language/typescript/ts.worker?worker";
interface monacostate {
  width?: string;
  height?: string;
  theme?: string;
  language?: string;
  selectOnLineNumbers?: string;
}

export default defineComponent({
  name: "monaco",

  emits: ['update:modelValue','change','focus','blur','paste','mouseup','mousedown','contentsizechange','keyup','keydown'],
  props: {
    modelValue: {
      type: String,
      default: "",
    },
    width: String,
    height: String,
    theme: String,
    language: String,
    selectOnLineNumbers: String,
  },

  setup(props, { emit }) {


    var container=ref();
    var isset = false;
    var newValue = computed({
      get: function () {
        return props.modelValue;
      },
      set: function (value) {
        isset = true;
        emit("update:modelValue", value);
      },
    });

    //   const content = ref(props.modelValue);
    // watch(
    //   () => props.modelValue,
    //   () => {
    //     content.value = props.modelValue;
    //     if (editor) {
    //       editor.setValue(content.value);
    //     }
    //   }
    // );


    //此处保证计算属性未能触发时能触发赋值，否则请按照明确的属性声明（props中定义）从父组件依次传值，可以保证正确的绑定
    watch(
      () => props.modelValue,
      () => {
        if (editor && !isset) {
          editor.setValue(newValue.value);
        }
      }
    );

    let editor: monaco.editor.IStandaloneCodeEditor;

    self.MonacoEnvironment = {
      getWorker(_, label) {
        if (label === "json") {
          return new jsonWorker();
        }
        if (label === "css" || label === "scss" || label === "less") {
          return new cssWorker();
        }
        if (label === "html" || label === "handlebars" || label === "razor") {
          return new htmlWorker();
        }
        if (label === "typescript" || label === "javascript") {
          return new tsWorker();
        }
        return new editorWorker();
      },
    };

    const state = reactive<monacostate>({
      width: props.width??'100%',
      height: props.height??'100px',
      theme: props.theme??'vs-dark',
      language: props.language,
      selectOnLineNumbers: props.selectOnLineNumbers,
    });
    onMounted(() => {
      editorInit();
    });
    const editorInit = () => {
      nextTick(() => {
        monaco.languages.typescript.javascriptDefaults.setDiagnosticsOptions({
          noSemanticValidation: true,
          noSyntaxValidation: false,
        });
        monaco.languages.typescript.javascriptDefaults.setCompilerOptions({
          target: monaco.languages.typescript.ScriptTarget.ES2016,
          allowNonTsExtensions: true,
        });

        !editor
          ? (editor = monaco.editor.create(
            container.value as HTMLElement,
              {
                value: newValue.value ?? "",
                language: state.language ?? "json",
                automaticLayout: true,
                theme: state.theme ?? "vs-dark", // 官方自带三种主题vs, hc-black, or vs-dark
                foldingStrategy: "indentation",
                renderLineHighlight: "all", // 行亮
                selectOnLineNumbers: state.selectOnLineNumbers == "true", // 显示行号
                minimap: {
                  enabled: false,
                },
                readOnly: false, // 只读
                fontSize: 16, // 字体大小
                scrollBeyondLastLine: false, // 取消代码后面一大段空白
                overviewRulerBorder: false, // 不要滚动条的边框
              }
            ))
          : editor.setValue(newValue.value ?? "");

        editor.onDidChangeModelContent((val: any) => {
          newValue.value = editor.getValue();
          emit("change", editor.getValue());
        });

        editor.onDidFocusEditorText(() => {
          emit("focus", editor);
        });

        editor.onDidBlurEditorText(() => {
          emit("blur", editor);
        });

        editor.onDidPaste((e: any) => {
          emit("paste", e);
        });

        editor.onMouseUp((e: any) => {
          emit("mouseup", e);
        });
        editor.onMouseDown((e: any) => {
          emit("mousedown", e);
        });
        editor.onDidContentSizeChange((e: any) => {
          emit("contentsizechange", e);
        });

        editor.onKeyUp((e: any) => {
          emit("keyup", e);
        });
        editor.onKeyDown((e: any) => {
          emit("keydown", e);
        });
      });
    };
    return {
      container,
      ...toRefs(state),
    };
  },
});
</script>
