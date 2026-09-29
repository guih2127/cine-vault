import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <div class="auth-page">
      <mat-card class="auth-card">
        <mat-card-header>
          <mat-card-title>Create your CineVault account</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline">
              <mat-label>Name</mat-label>
              <input matInput formControlName="name" autocomplete="name" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Email</mat-label>
              <input matInput type="email" formControlName="email" autocomplete="email" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Password</mat-label>
              <input matInput type="password" formControlName="password" autocomplete="new-password" />
            </mat-form-field>
            @if (error()) {
              <p class="error">{{ error() }}</p>
            }
            <button mat-flat-button color="primary" type="submit" [disabled]="loading() || form.invalid">
              {{ loading() ? 'Creating…' : 'Register' }}
            </button>
          </form>
        </mat-card-content>
        <mat-card-actions>
          <a routerLink="/login">Already have an account? Sign in</a>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: `
    .auth-page { display: flex; justify-content: center; padding: 2rem 1rem; }
    .auth-card { width: 100%; max-width: 400px; }
    form { display: flex; flex-direction: column; }
    .error { color: var(--mat-sys-error, #b3261e); margin: 0 0 1rem; }
  `
})
export class Register {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  readonly error = signal<string | null>(null);
  readonly loading = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  async submit(): Promise<void> {
    if (this.form.invalid) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    try {
      const { name, email, password } = this.form.getRawValue();
      await this.authService.register(name, email, password);
    } catch (error: unknown) {
      const conflict = (error as { status?: number }).status === 409;
      this.error.set(conflict ? 'This email is already registered.' : 'Could not create the account.');
    } finally {
      this.loading.set(false);
    }
  }
}
