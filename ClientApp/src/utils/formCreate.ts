import formCreate, { type Api } from '@form-create/element-ui';
import install from '@form-create/element-ui/auto-import';

formCreate.use(install);

export type { Api };
export default formCreate;
