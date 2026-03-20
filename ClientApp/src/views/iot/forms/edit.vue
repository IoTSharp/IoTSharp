<template><div id="codeEditBox" style="height: 300px"></div></template>

<script lang="ts">
import { ref, toRefs, reactive, onMounted, defineComponent ,nextTick} from "vue";
import { ElMessageBox, ElMessage } from "element-plus";
import { loadMonacoEditor } from "/@/utils/monacoLoader";
import {useRoute} from 'vue-router'

type MonacoEditor = import("monaco-editor/esm/vs/editor/editor.api").editor.IStandaloneCodeEditor;

export default defineComponent({
  name: 'addDevice',
  components: {},
  setup() {
    const text=ref('')
const route=useRoute()
const language=ref('go')
const msg=ref()
const loading=ref(false)
let editor: MonacoEditor | undefined;

const editorInit = async () => {
    await nextTick();

    const monaco = await loadMonacoEditor(language.value);
    const editorContainer = document.getElementById('codeEditBox');
    if (!editorContainer) return;

    if (!editor) {
        editor = monaco.editor.create(editorContainer, {
            value:text.value, // 缂栬緫鍣ㄥ垵濮嬫樉绀烘枃瀛?
            language: language.value, // 璇█鏀寔鑷鏌ラ槄demo
            automaticLayout: true, // 鑷€傚簲甯冨眬
            theme: 'vs-dark', // 瀹樻柟鑷甫涓夌涓婚vs, hc-black, or vs-dark
            foldingStrategy: 'indentation',
            renderLineHighlight: 'all', // 琛屼寒
            selectOnLineNumbers: true, // 鏄剧ず琛屽彿
            minimap:{
                enabled: false,
            },
            readOnly: false, // 鍙
            fontSize: 16, // 瀛椾綋澶у皬
            scrollBeyondLastLine: false, // 鍙栨秷浠ｇ爜鍚庨潰涓€澶ф绌虹櫧
            overviewRulerBorder: false, // 涓嶈婊氬姩鏉＄殑杈规
        });
        editor.onDidChangeModelContent(() => {
            text.value = editor?.getValue() ?? "";
        })
        return;
    }

    editor.setValue("");
}
    onMounted(() => {


        void editorInit()
    });
    return {};
  },
});
</script>
<style lang="scss">
body {
  margin: 0;  /* 濡傛灉椤甸潰鍑虹幇鍨傜洿婊氬姩鏉★紝鍒欏姞鍏ユ琛孋SS浠ユ秷闄や箣 */
}

</style>
