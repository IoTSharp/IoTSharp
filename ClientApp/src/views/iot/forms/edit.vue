<template><div id="codeEditBox" style="height: 300px"></div></template>

<script lang="ts">
import { ref, toRefs, reactive, onMounted, defineComponent ,nextTick} from "vue";
import { ElMessageBox, ElMessage } from "element-plus";
import jsonWorker from 'monaco-editor/esm/vs/language/json/json.worker?worker'
import cssWorker from 'monaco-editor/esm/vs/language/css/css.worker?worker'
import htmlWorker from 'monaco-editor/esm/vs/language/html/html.worker?worker'
import tsWorker from 'monaco-editor/esm/vs/language/typescript/ts.worker?worker'
import EditorWorker from 'monaco-editor/esm/vs/editor/editor.worker?worker'
import * as monaco from 'monaco-editor';
import {useRoute} from 'vue-router'
export default defineComponent({
  name: 'addDevice',
  components: {},
  setup() {
    const text=ref('')
const route=useRoute()
const language=ref('go')
const msg=ref()
const loading=ref(false)
let editor: monaco.editor.IStandaloneCodeEditor;


const editorInit = () => {
    nextTick(()=>{
        monaco.languages.typescript.javascriptDefaults.setDiagnosticsOptions({
            noSemanticValidation: true,
            noSyntaxValidation: false
        });
        monaco.languages.typescript.javascriptDefaults.setCompilerOptions({
            target: monaco.languages.typescript.ScriptTarget.ES2016,
            allowNonTsExtensions: true
        });
        
        !editor ? editor = monaco.editor.create(document.getElementById('codeEditBox') as HTMLElement, {
            value:text.value, // 编辑器初始显示文字
            language: 'go', // 语言支持自行查阅demo
            automaticLayout: true, // 自适应布局  
            theme: 'vs-dark', // 官方自带三种主题vs, hc-black, or vs-dark
            foldingStrategy: 'indentation',
            renderLineHighlight: 'all', // 行亮
            selectOnLineNumbers: true, // 显示行号
            minimap:{
                enabled: false,
            },
            readOnly: false, // 只读
            fontSize: 16, // 字体大小
            scrollBeyondLastLine: false, // 取消代码后面一大段空白 
            overviewRulerBorder: false, // 不要滚动条的边框  
        }) : 
        editor.setValue("");
        // console.log(editor)
        // 监听值的变化
        editor.onDidChangeModelContent((val:any) => {
            text.value = editor.getValue();
             
        })
    })
}
    onMounted(() => {


        editorInit()
    });
    return {};
  },
});
</script>
<style lang="scss">
body {
  margin: 0;  /* 如果页面出现垂直滚动条，则加入此行CSS以消除之 */
}

</style>