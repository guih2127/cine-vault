import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';
import { MoviesService } from '../../core/api/movies.service';
import { ReviewsService } from '../../core/api/reviews.service';
import { Movie } from '../../core/models/movie.model';
import { Review } from '../../core/models/review.model';
import { AddMovieDialog, AddMovieResult } from './add-movie-dialog';

@Component({
  selector: 'app-movies',
  imports: [
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  template: `
    <header class="page-header">
      <div>
        <h1>My Movies</h1>
        <p>Rate and keep track of the movies you've watched.</p>
      </div>
      <button mat-flat-button color="primary" (click)="openAddDialog()">
        <mat-icon>add</mat-icon>
        Add movie
      </button>
    </header>

    @if (loading()) {
      <div class="center"><mat-spinner diameter="48" /></div>
    } @else if (myMovies().length === 0) {
      <div class="empty">
        <mat-icon>movie</mat-icon>
        <p>You haven't rated any movies yet.</p>
        <button mat-flat-button color="primary" (click)="openAddDialog()">
          <mat-icon>add</mat-icon>
          Add your first movie
        </button>
      </div>
    } @else {
      <div class="grid">
        @for (card of myMovies(); track card.movie.id) {
          <mat-card class="movie-card" appearance="outlined">
            @if (card.movie.posterUrl) {
              <img [src]="card.movie.posterUrl" [alt]="card.movie.title" class="poster" />
            }
            <div class="body">
              <h3 class="title">{{ card.movie.title }}</h3>
              <span class="year">{{ card.movie.year }}</span>

              <mat-form-field appearance="outline" class="rating-field" subscriptSizing="dynamic">
                <mat-label>Your rating</mat-label>
                <mat-icon matPrefix class="star rated">star</mat-icon>
                <mat-select
                  [value]="draftRatings().get(card.movie.id) ?? card.review.rating"
                  (selectionChange)="setDraft(card.movie.id, $event.value)">
                  @for (option of ratingOptions; track option) {
                    <mat-option [value]="option">{{ option }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>

              <div class="actions">
                <button mat-button color="warn" (click)="remove(card.review)">Remove</button>
                <button mat-flat-button color="primary" (click)="updateRating(card.review)">Update</button>
              </div>
            </div>
          </mat-card>
        }
      </div>
    }
  `,
  styles: `
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1rem;
      margin: 0.5rem 0 1.75rem;
    }
    .page-header h1 { margin: 0; font-size: 1.6rem; font-weight: 600; }
    .page-header p { margin: 0.25rem 0 0; color: rgba(0, 0, 0, 0.6); }

    .center { display: flex; justify-content: center; padding: 3rem; }

    .empty {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.75rem;
      padding: 4rem 1rem;
      color: rgba(0, 0, 0, 0.6);
    }
    .empty mat-icon { font-size: 48px; width: 48px; height: 48px; opacity: 0.4; }

    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(230px, 1fr));
      gap: 1.5rem;
    }

    .movie-card { padding: 0; overflow: hidden; border-radius: 12px; }
    .poster { width: 100%; height: 155px; object-fit: cover; object-position: center top; display: block; }
    .body { display: flex; flex-direction: column; padding: 1rem 1rem 1.1rem; }
    .title { margin: 0; font-size: 1.05rem; font-weight: 600; line-height: 1.3; }
    .year { color: rgba(0, 0, 0, 0.55); font-size: 0.85rem; margin: 0.15rem 0 1.1rem; }

    .rating-field { width: 100%; }
    .star { color: rgba(0, 0, 0, 0.3); }
    .star.rated { color: #f5a623; }

    .actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.1rem; }
  `
})
export class Movies implements OnInit {
  private readonly moviesService = inject(MoviesService);
  private readonly reviewsService = inject(ReviewsService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  private readonly movies = signal<Movie[]>([]);
  private readonly reviews = signal<Review[]>([]);

  readonly loading = signal(true);
  readonly draftRatings = signal(new Map<string, number>());
  readonly ratingOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

  readonly myMovies = computed(() => {
    const reviews = this.reviews();
    return this.movies()
      .map((movie) => ({ movie, review: reviews.find((review) => review.movieId === movie.id) }))
      .filter((card): card is { movie: Movie; review: Review } => card.review !== undefined);
  });

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  async openAddDialog(): Promise<void> {
    const reviewedMovieIds = new Set(this.reviews().map((review) => review.movieId));
    const available = this.movies().filter((movie) => !reviewedMovieIds.has(movie.id));

    const dialogRef = this.dialog.open(AddMovieDialog, { data: { available } });
    const result = await firstValueFrom<AddMovieResult | undefined>(dialogRef.afterClosed());

    if (result) {
      await this.run(() => this.reviewsService.create(result.movieId, result.rating), 'Review added');
    }
  }

  setDraft(movieId: string, rating: number): void {
    this.draftRatings.update((drafts) => new Map(drafts).set(movieId, rating));
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
