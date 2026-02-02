import { Component, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-failed',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './failed.html',
  styleUrl: './failed.css',
})
export class Failed {
  private router = inject(Router);

  tryAnotherMethod() {
    this.router.navigate(['/index']);
  }
}
