<template>

  <div :class="{ mncfullscreen: isfullscreen,mnc:!isfullscreen }">
    <div ref="container" :style="{ height: props.height, width: props.width ,minHeight:'300px'}">
      <el-icon style="position:absolute; z-index:999; top: 40px; right: 30px; cursor: pointer; " color="#c6e2ff" @click="toggle" >
        <FullScreen />
      </el-icon>
    </div>
  </div>

</template>
<script lang="ts" setup>
import { ref, nextTick } from "vue";

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


const emit = defineEmits(["update:modelValue", "change", "focus", "blur", "paste", "mouseup", "mousedown", "contentsizechange", "keyup", "keydown"]);
const props = defineProps({
  modelValue: {
    type: String,
    default: "",
  },
  width: String,
  height: String,
  theme: String,
  language: String,
  selectOnLineNumbers: String,
});




var isuserinput = false;
const container = ref();
const isfullscreen = ref(false);

watch(
  () => props.modelValue,
  () => {
    if (editor && !isuserinput) {
      editor.setValue(props.modelValue ?? "");
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

const toggle = () => {
  isfullscreen.value=!isfullscreen.value;
}

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
      ? (editor = monaco.editor.create(container.value as HTMLElement, {
        value: props.modelValue ?? "",
        language: props.language?? "json",
        automaticLayout: true,
        theme: props.theme ?? "vs-dark", // 官方自带三种主题vs, hc-black, or vs-dark
        foldingStrategy: "indentation",
        renderLineHighlight: "all", // 行亮
        selectOnLineNumbers: props.selectOnLineNumbers == "true", // 显示行号
        minimap: {
          enabled: false,
        },
        readOnly: false, // 只读
        fontSize: 16, // 字体大小
        scrollBeyondLastLine: false, // 取消代码后面一大段空白
        overviewRulerBorder: false, // 不要滚动条的边框
      }))
      : editor.setValue(props.modelValue ?? "");

    editor.onDidChangeModelContent((val: any) => {
      emit("update:modelValue", editor.getValue());
      // newValue.value = editor.getValue();
      //  emit("change", editor.getValue());
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
      isuserinput = false;
      emit("mouseup", e);
    });
    editor.onMouseDown((e: any) => {
      isuserinput = true;
      emit("mousedown", e);
    });
    editor.onDidContentSizeChange((e: any) => {
      emit("contentsizechange", e);
    });

    editor.onKeyUp((e: any) => {
      isuserinput = false;
      emit("keyup", e);
    });
    editor.onKeyDown((e: any) => {
      isuserinput = true;
      emit("keydown", e);
    });
  });
};


</script>


<style scoped lang="scss">
.mncfullscreen {
  position: fixed;
  width: 100%; height: 100%;    padding: 5px;
  top: 0;
  left: 0; z-index: 999;
}

.mnc{
  width: 100%; 
  float: left;
  height: 100%;
 position: relative;
}
</style>
