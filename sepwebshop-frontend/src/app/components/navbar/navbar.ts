import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { Auth } from '../../services/auth/auth';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit {
  mobileOpen = false;
  showNavbar = true;

  hiddenRoutes = ['/login', '/register', '/success', '/error', '/failed'];

  constructor(
    public auth: Auth,
    private router: Router,
  ) {}

  ngOnInit() {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.showNavbar = !this.hiddenRoutes.includes(event.urlAfterRedirects);
      });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
