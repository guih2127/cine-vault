import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Movie } from '../models/movie.model';

@Injectable({ providedIn: 'root' })
export class MoviesService {
  private readonly http = inject(HttpClient);

  getMovies(): Promise<Movie[]> {
    return firstValueFrom(this.http.get<Movie[]>(`${environment.apiUrl}/movies`));
  }
}
