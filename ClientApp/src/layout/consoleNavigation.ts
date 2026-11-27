import type { InjectionKey, Ref } from 'vue';

export const consoleNavigationKey: InjectionKey<{
	isMobile: Ref<boolean>;
	open: Ref<boolean>;
	toggle: () => void;
}> = Symbol('consoleNavigation');
