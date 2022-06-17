import { NgModule } from '@angular/core';
import { SharedModule } from '@shared';
import { WidgetsModule } from '../widgets/widgets.module';

import { CallbackComponent } from './callback.component';
import { UserLockComponent } from './lock/lock.component';
import { UserLoginComponent } from './login/login.component';
import { PassportRoutingModule } from './passport-routing.module';
import { UserRegisterResultComponent } from './register-result/register-result.component';
import { UserRegisterComponent } from './register/register.component';

const COMPONENTS = [UserLoginComponent, UserRegisterResultComponent, UserRegisterComponent, UserLockComponent, CallbackComponent];

@NgModule({
  imports: [SharedModule, PassportRoutingModule, WidgetsModule],
  declarations: [...COMPONENTS]
})
export class PassportModule {}
