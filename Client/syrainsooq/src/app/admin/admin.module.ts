import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminRoutingModule } from './admin-routing.module';
import { CategoryComponent } from './components/category/category.component';
import { DashboardLayoutComponent } from './components/dashboard-layout/dashboard-layout.component';

@NgModule({
  declarations: [
    CategoryComponent,
    DashboardLayoutComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    AdminRoutingModule
  ],
})
export class AdminModule {}
