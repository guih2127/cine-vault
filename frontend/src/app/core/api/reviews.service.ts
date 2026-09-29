import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Review } from '../models/review.model';

@Injectable({ providedIn: 'root' })
export class ReviewsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/reviews`;

  getMyReviews(): Promise<Review[]> {
    return firstValueFrom(this.http.get<Review[]>(this.baseUrl));
  }

  create(movieId: string, rating: number): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(this.baseUrl, { movieId, rating }));
  }

  update(reviewId: string, rating: number): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${this.baseUrl}/${reviewId}`, { rating }));
  }

  delete(reviewId: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.baseUrl}/${reviewId}`));
  }
}
