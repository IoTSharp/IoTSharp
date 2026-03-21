<template>
  <div class="app-logo" :class="{ 'app-logo--icon-only': hideText, 'app-logo--text-only': hideIcon }">
    <div v-if="!hideIcon" class="app-logo__mark" aria-hidden="true">
      <span class="app-logo__shape app-logo__shape--primary"></span>
      <span class="app-logo__shape app-logo__shape--secondary"></span>
    </div>
    <div v-if="!hideText" class="app-logo__wordmark">
      <span class="app-logo__prefix">{{ titleParts.prefix }}</span>
      <span v-if="titleParts.suffix" class="app-logo__suffix">{{ titleParts.suffix }}</span>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';

defineProps({
  hideIcon: {
    type: Boolean,
    default: false,
  },
  hideText: {
    type: Boolean,
    default: false,
  },
});

const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);

const titleParts = computed(() => {
  const rawTitle = (themeConfig.value.globalTitle || 'IoTSharp').trim();

  if (/^iot\s*sharp$/i.test(rawTitle)) {
    return { prefix: 'IoT', suffix: 'Sharp' };
  }

  if (/^iot/i.test(rawTitle)) {
    const suffix = rawTitle.slice(3).trim() || 'Sharp';
    return { prefix: 'IoT', suffix };
  }

  return { prefix: rawTitle, suffix: '' };
});
</script>

<style lang="scss" scoped>
.app-logo {
  --app-logo-text: #1d2129;
  --app-logo-subtext: #4e5969;
  display: inline-flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
  user-select: none;
}

.app-logo--icon-only {
  justify-content: center;
}

.app-logo__mark {
  position: relative;
  width: 30px;
  height: 22px;
  flex-shrink: 0;
}

.app-logo__shape {
  position: absolute;
  display: block;
  width: 17px;
  height: 17px;
  border-radius: 6px;
  transform: rotate(45deg);
  box-shadow: 0 8px 18px rgba(22, 93, 255, 0.12);
}

.app-logo__shape--primary {
  top: 0;
  left: 0;
  background: linear-gradient(135deg, #00b2ff 0%, #36cfc9 100%);
}

.app-logo__shape--secondary {
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, #165dff 0%, #4080ff 100%);
}

.app-logo__wordmark {
  display: inline-flex;
  align-items: baseline;
  gap: 4px;
  min-width: 0;
  color: var(--app-logo-text);
  font-family: 'Segoe UI Variable', 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  font-size: 28px;
  font-weight: 700;
  line-height: 1;
  letter-spacing: -0.03em;
  white-space: nowrap;
}

.app-logo__suffix {
  color: var(--app-logo-subtext);
  font-weight: 600;
}

@media (max-width: 767px) {
  .app-logo {
    gap: 10px;
  }

  .app-logo__mark {
    width: 28px;
    height: 20px;
  }

  .app-logo__shape {
    width: 15px;
    height: 15px;
    border-radius: 5px;
  }

  .app-logo__wordmark {
    font-size: 24px;
  }
}
</style>
