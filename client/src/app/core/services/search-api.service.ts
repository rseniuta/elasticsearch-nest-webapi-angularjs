import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PostDetail, PostDetailDto, SearchHit, SearchHitDto, SearchResponseDto, SearchResult } from '../models/search.models';

@Injectable({ providedIn: 'root' })
export class SearchApiService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  search(query: string): Observable<SearchResult> {
    return this.http
      .get<SearchResponseDto>(`${this.baseUrl}/search`, { params: { q: query } })
      .pipe(map((dto) => this.mapSearchResponse(dto)));
  }

  searchByCategories(query: string, categories: string[]): Observable<SearchResult> {
    return this.http
      .post<SearchResponseDto>(`${this.baseUrl}/searchbycategory`, { q: query, categories })
      .pipe(map((dto) => this.mapSearchResponse(dto)));
  }

  index(fileName: string, maxItems = 1000): Observable<void> {
    return this.http.get<void>(`${this.baseUrl}/index`, { params: { fileName, maxItems } });
  }

  suggest(query: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/suggest`, { params: { q: query } });
  }

  autocomplete(prefix: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/autocomplete`, { params: { q: prefix } });
  }

  getPost(id: string): Observable<PostDetail> {
    return this.http
      .get<PostDetailDto>(`${this.baseUrl}/get`, { params: { id } })
      .pipe(map((dto) => this.mapPost(dto)));
  }

  moreLikeThis(id: string, pageSize = 3): Observable<SearchHit[]> {
    return this.http
      .get<SearchResponseDto>(`${this.baseUrl}/morelikethis`, { params: { id, pageSize } })
      .pipe(map((dto) => this.mapSearchResponse(dto).results));
  }

  private mapSearchResponse(dto: SearchResponseDto): SearchResult {
    return {
      total: dto.Total,
      took: dto.ElapsedMilliseconds,
      aggregationsByTags: dto.AggregationsByTags ?? {},
      results: (dto.Results ?? []).map((item) => this.mapHit(item))
    };
  }

  private mapPost(dto: PostDetailDto): PostDetail {
    return {
      ...this.mapHit(dto),
      creationDate: dto.CreationDate ?? undefined,
      similar: (dto.Similar ?? []).map((item) => this.mapHit(item))
    };
  }

  private mapHit(item: SearchHitDto): SearchHit {
    return {
      id: item.Id,
      title: item.Title,
      body: item.Body,
      tags: item.Tags ?? [],
      score: item.Score ?? 0,
      answerCount: item.AnswerCount ?? 0
    };
  }
}
