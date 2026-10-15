import { Component } from '@angular/core';
import { SiteLayoutComponent } from './layout/site-layout/site-layout.component';

@Component({
  selector: 'app-root',
  imports: [SiteLayoutComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {}

