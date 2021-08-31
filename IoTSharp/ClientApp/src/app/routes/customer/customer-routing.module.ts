import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { CustomerformComponent } from "./customerform/customerform.component";
import { CustomerlistComponent } from "./customerlist/customerlist.component";

const routes: Routes = [

    { path: 'customerlist', component: CustomerlistComponent },
    { path: 'customerlform', component: CustomerformComponent },

];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule],
})
export class CustomerRoutingModule { }
