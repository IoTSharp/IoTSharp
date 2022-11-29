<template>
  <div id="codeEditBox" :style="{ height: height, width: width }"></div>
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
import { ElMessage } from "element-plus";
import * as monaco from "monaco-editor";
interface monacostate {
  width?: string;
  height?: string;
  value?: string;
  theme?: string;
  language?:string;
  selectOnLineNumbers?:string;
}

export default defineComponent({
  name: "monaco",
  props: {
    width: String,
    height: String,
    value: String,
    theme: String,
    language:String,
    selectOnLineNumbers:String
  },
  setup(props, { emit }) {
    let editor: monaco.editor.IStandaloneCodeEditor;
    const state = reactive<monacostate>({
      width: props.width,
      height: props.height,
      value: props.value,
      theme: props.theme,
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
              document.getElementById("codeEditBox") as HTMLElement,
              {
                value: state.value ?? '',
                language: state.language??'json',
                automaticLayout: true,
                theme: state.theme ??'vs', // 官方自带三种主题vs, hc-black, or vs-dark
                foldingStrategy: "indentation",
                renderLineHighlight: "all", // 行亮
                selectOnLineNumbers: state.selectOnLineNumbers=='true' , // 显示行号
                minimap: {
                  enabled: false,
                },
                readOnly: false, // 只读
                fontSize: 16, // 字体大小
                scrollBeyondLastLine: false, // 取消代码后面一大段空白
                overviewRulerBorder: false, // 不要滚动条的边框
              }
            ))
          : editor.setValue(state.value ?? "");
        // console.log(editor)
        // 监听值的变化
        editor.onDidChangeModelContent((val: any) => {
          state.value = editor.getValue();
        });
      });
    };
    return {
      ...toRefs(state),
    };
  },
});
</script>
