import { NgModule } from '@angular/core';
import {
  BrowserModule,
  provideClientHydration,
  withEventReplay,
} from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { AuthService } from './identity/services/auth.service';
import { credentialsInterceptor } from './core/interceptor/credentials.interceptor';
import { authInterceptor } from './identity/interceptors/auth.interceptor';

@NgModule({
  declarations: [AppComponent, HomeComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    ToastrModule.forRoot({
      timeOut: 5000,
      positionClass: 'toast-top-right',
      preventDuplicates: true,
      closeButton: true,
      progressBar: true,
      enableHtml: true,
    }),
    AppRoutingModule,
  ],
  providers: [
    provideClientHydration(withEventReplay()),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor, credentialsInterceptor])
    ),
    AuthService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
