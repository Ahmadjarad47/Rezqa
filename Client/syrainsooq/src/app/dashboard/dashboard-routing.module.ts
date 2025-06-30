import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from './components/layout/layout.component';
import { SettingsComponent } from './components/settings/settings.component';
import { WishlistComponent } from './components/wishlist/wishlist.component';
import { AdsComponent } from './components/ads/ads.component';
import { AuthGuard } from '@app/admin/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    canActivate:[AuthGuard],
    children: [
      { path: '', redirectTo: 'ads', pathMatch: 'full' },
      { path: 'ads', component: AdsComponent },
      { path: 'wishlist', component: WishlistComponent },
      { path: 'settings', component: SettingsComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
