import { Component, OnInit } from '@angular/core';
import { FlowbiteService } from './core/services/flowbite.service';
import { initFlowbite } from 'flowbite';
import { AuthService } from './identity/services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  title = 'SyrianOpenSooq';

  constructor(private flowbiteService: FlowbiteService,private auth:AuthService) {}

  ngOnInit(): void {
    this.flowbiteService.loadFlowbite((flowbite) => {
      initFlowbite();
    });
    this.auth.getData().subscribe();
  }
}
