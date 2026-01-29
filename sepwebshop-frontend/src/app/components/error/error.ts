import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Location } from '@angular/common';

@Component({
  selector: 'app-error',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './error.html',
  styleUrl: './error.css',
})
export class Error {
  private location = inject(Location);

  goBack() {
    this.location.back(); // Vraća korisnika na prethodnu stranicu
  }
}
