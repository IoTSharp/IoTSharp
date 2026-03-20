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
import { loadMonacoEditor } from "/@/utils/monacoLoader";

type MonacoEditor = import("monaco-editor/esm/vs/editor/editor.api").editor.IStandaloneCodeEditor;

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

let editor: MonacoEditor | undefined;

const toggle = () => {
  isfullscreen.value = !isfullscreen.value;
}

onMounted(() => {
  void editorInit();
});

const editorInit = async () => {
  await nextTick();

  const monaco = await loadMonacoEditor(props.language ?? "json");
  if (!container.value) return;

  if (!editor) {
    editor = monaco.editor.create(container.value as HTMLElement, {
      value: props.modelValue ?? "",
      language: props.language ?? "json",
      automaticLayout: true,
      theme: props.theme ?? "vs-dark", // 瀹樻柟鑷甫涓夌涓婚vs, hc-black, or vs-dark
      foldingStrategy: "indentation",
      renderLineHighlight: "all", // 琛屼寒
      selectOnLineNumbers: props.selectOnLineNumbers == "true", // 鏄剧ず琛屽彿
      minimap: {
        enabled: false,
      },
      readOnly: false, // 鍙
      fontSize: 16, // 瀛椾綋澶у皬
      scrollBeyondLastLine: false, // 鍙栨秷浠ｇ爜鍚庨潰涓€澶ф绌虹櫧
      overviewRulerBorder: false, // 涓嶈婊氬姩鏉＄殑杈规
    });

    editor.onDidChangeModelContent(() => {
      emit("update:modelValue", editor?.getValue() ?? "");
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
    return;
  }

  editor.setValue(props.modelValue ?? "");
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
