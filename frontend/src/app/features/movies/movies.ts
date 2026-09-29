import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MoviesService } from '../../core/api/movies.service';
import { ReviewsService } from '../../core/api/reviews.service';
import { Movie } from '../../core/models/movie.model';
import { Review } from '../../core/models/review.model';

@Component({
  selector: 'app-movies',
  imports: [MatCardModule, MatButtonModule, MatFormFieldModule, MatSelectModule, MatProgressSpinnerModule],
  template: `
    @if (loading()) {
      <div class="center"><mat-spinner diameter="48" /></div>
    } @else {
      <div class="grid">
        @for (card of cards(); track card.movie.id) {
          <mat-card class="movie-card">
            @if (card.movie.posterUrl) {
              <img [src]="card.movie.posterUrl" [alt]="card.movie.title" class="poster" />
            }
            <mat-card-header>
              <mat-card-title>{{ card.movie.title }}</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <mat-form-field appearance="outline" class="rating-field">
                <mat-label>Your rating</mat-label>
                <mat-select
                  [value]="draftRatings().get(card.movie.id) ?? card.review?.rating ?? null"
                  (selectionChange)="setDraft(card.movie.id, $event.value)">
                  @for (option of ratingOptions; track option) {
                    <mat-option [value]="option">{{ option }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>
            </mat-card-content>
            <mat-card-actions align="end">
              @if (card.review) {
                <button mat-button color="warn" (click)="remove(card.review)">Remove</button>
                <button mat-flat-button color="primary" (click)="updateRating(card.review)">Update</button>
              } @else {
                <button mat-flat-button color="primary" (click)="rate(card.movie.id)">Rate</button>
              }
            </mat-card-actions>
          </mat-card>
        }
      </div>
    }
  `,
  styles: `
    .center { display: flex; justify-content: center; padding: 3rem; }
    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 1rem; }
    .movie-card { display: flex; flex-direction: column; }
    .poster { width: 100%; height: 300px; object-fit: cover; }
    .rating-field { width: 100%; }
  `
})
export class Movies implements OnInit {
  private readonly moviesService = inject(MoviesService);
  private readonly reviewsService = inject(ReviewsService);
  private readonly snackBar = inject(MatSnackBar);

  private readonly movies = signal<Movie[]>([]);
  private readonly reviews = signal<Review[]>([]);

  readonly loading = signal(true);
  readonly draftRatings = signal(new Map<string, number>());
  readonly ratingOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

  readonly cards = computed(() =>
    this.movies().map((movie) => ({
      movie,
      review: this.reviews().find((review) => review.movieId === movie.id) ?? null
    }))
  );

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  setDraft(movieId: string, rating: number): void {
    this.draftRatings.update((drafts) => new Map(drafts).set(movieId, rating));
  }

  async rate(movieId: string): Promise<void> {
    const rating = this.draftRatings().get(movieId);
    if (!rating) {
      this.snackBar.open('Pick a rating first.', 'OK', { duration: 2500 });
      return;
    }
    await this.run(() => this.reviewsService.create(movieId, rating), 'Review added');
  }

  async updateRating(review: Review): Promise<void> {
    const rating = this.draftRatings().get(review.movieId) ?? review.rating!;
    await this.run(() => this.reviewsService.update(review.id, rating), 'Review updated');
  }

  async remove(review: Review): Promise<void> {
    await this.run(() => this.reviewsService.delete(review.id), 'Review removed');
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    const [movies, reviews] = await Promise.all([
      this.moviesService.getMovies(),
      this.reviewsService.getMyReviews()
    ]);
    this.movies.set(movies);
    this.reviews.set(reviews);
    this.loading.set(false);
  }

  private async run(action: () => Promise<unknown>, successMessage: string): Promise<void> {
    try {
      await action();
      await this.load();
      this.snackBar.open(successMessage, 'OK', { duration: 2500 });
    } catch {
      this.snackBar.open('Something went wrong.', 'OK', { duration: 3000 });
    }
  }
}
