<template>
  <div class="app-logo" :class="{ 'app-logo--icon-only': hideText, 'app-logo--text-only': hideIcon }">
    <div v-if="!hideIcon" class="app-logo__mark" aria-hidden="true">
      <span class="app-logo__hash">#</span>
      <span class="app-logo__spark app-logo__spark--top"></span>
      <span class="app-logo__spark app-logo__spark--bottom"></span>
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
  --app-logo-text: #123b6d;
  --app-logo-subtext: #2563eb;
  display: inline-flex;
  align-items: center;
  gap: 14px;
  min-width: 0;
  user-select: none;
}

.app-logo--icon-only {
  justify-content: center;
}

.app-logo__mark {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 38px;
  height: 38px;
  border-radius: 11px;
  background: linear-gradient(180deg, #1d4ed8 0%, #2563eb 52%, #0ea5e9 100%);
  box-shadow:
    0 12px 22px rgba(37, 99, 235, 0.24),
    inset 0 1px 0 rgba(255, 255, 255, 0.3);
  flex-shrink: 0;
}

.app-logo__hash {
  position: relative;
  z-index: 2;
  color: #ffffff;
  font-size: 25px;
  font-weight: 800;
  line-height: 1;
  transform: translateY(-1px);
}

.app-logo__spark {
  position: absolute;
  right: 6px;
  width: 4px;
  height: 4px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.92);
  box-shadow: 0 0 0 5px rgba(255, 255, 255, 0.08);
}

.app-logo__spark--top {
  top: 8px;
}

.app-logo__spark--bottom {
  bottom: 8px;
}

.app-logo__wordmark {
  display: inline-flex;
  align-items: baseline;
  gap: 6px;
  min-width: 0;
  color: var(--app-logo-text);
  font-family: 'Segoe UI', 'Helvetica Neue', 'Microsoft YaHei', sans-serif;
  font-size: 27px;
  font-weight: 800;
  letter-spacing: -0.05em;
  line-height: 0.95;
  white-space: nowrap;
}

.app-logo__prefix,
.app-logo__suffix {
  display: inline-block;
}

.app-logo__suffix {
  color: var(--app-logo-subtext);
  letter-spacing: -0.07em;
}

@media (max-width: 767px) {
  .app-logo {
    gap: 10px;
  }

  .app-logo__mark {
    width: 34px;
    height: 34px;
    border-radius: 10px;
  }

  .app-logo__hash {
    font-size: 22px;
  }

  .app-logo__wordmark {
    font-size: 24px;
  }
}
</style>
