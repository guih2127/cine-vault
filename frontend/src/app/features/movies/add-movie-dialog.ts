import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { Movie } from '../../core/models/movie.model';

export interface AddMovieResult {
  movieId: string;
  rating: number;
}

@Component({
  selector: 'app-add-movie-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatSelectModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Add a movie</h2>
    <mat-dialog-content>
      @if (data.available.length === 0) {
        <p class="empty-note">You've already rated every movie in the catalog. 🎉</p>
      } @else {
        <form [formGroup]="form" class="form">
          <mat-form-field appearance="outline">
            <mat-label>Movie</mat-label>
            <mat-select formControlName="movieId">
              @for (movie of data.available; track movie.id) {
                <mat-option [value]="movie.id">{{ movie.title }} ({{ movie.year }})</mat-option>
              }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Rating</mat-label>
            <mat-select formControlName="rating">
              @for (option of ratingOptions; track option) {
                <mat-option [value]="option">{{ option }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        </form>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" [disabled]="form.invalid" (click)="add()">Add</button>
    </mat-dialog-actions>
  `,
  styles: `
    .form { display: flex; flex-direction: column; gap: 0.5rem; min-width: 320px; padding-top: 0.5rem; }
    .empty-note { min-width: 300px; }
  `
})
export class AddMovieDialog {
  private readonly dialogRef = inject(MatDialogRef<AddMovieDialog, AddMovieResult>);
  private readonly formBuilder = inject(FormBuilder);
  readonly data = inject<{ available: Movie[] }>(MAT_DIALOG_DATA);

  readonly ratingOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

  readonly form = this.formBuilder.group({
    movieId: ['', Validators.required],
    rating: [null as number | null, Validators.required]
  });

  add(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({ movieId: value.movieId!, rating: value.rating! });
  }
}
