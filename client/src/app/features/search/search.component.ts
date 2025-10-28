import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { BehaviorSubject, Observable, Subject, combineLatest, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, finalize, map, shareReplay, startWith, switchMap, takeUntil } from 'rxjs/operators';

import { PostDetail, SearchHit, SearchResult } from '../../core/models/search.models';
import { SearchApiService } from '../../core/services/search-api.service';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchComponent implements OnDestroy {
  private readonly destroy$ = new Subject<void>();

  readonly searchForm = this.fb.group({
    query: ['', [Validators.required, Validators.minLength(2)]]
  });

  readonly activeFilters$ = new BehaviorSubject<Set<string>>(new Set<string>());
  readonly results$ = new BehaviorSubject<SearchResult | null>(null);
  readonly isLoading$ = new BehaviorSubject<boolean>(false);
  readonly message$ = new BehaviorSubject<string>('');
  readonly suggestedTerms$ = new BehaviorSubject<string[]>([]);
  readonly selectedPost$ = new BehaviorSubject<PostDetail | null>(null);
  readonly similarPosts$ = new BehaviorSubject<SearchHit[]>([]);

  readonly autocomplete$: Observable<string[]> = this.searchForm.controls.query.valueChanges.pipe(
    startWith(this.searchForm.controls.query.value ?? ''),
    debounceTime(250),
    distinctUntilChanged(),
    switchMap((value) => this.loadAutocomplete(value ?? '')),
    shareReplay(1)
  );

  readonly summary$ = combineLatest([
    this.results$,
    this.message$
  ]).pipe(
    map(([result, message]) => ({
      total: result?.total ?? 0,
      took: result?.took ?? 0,
      message
    }))
  );

  readonly aggregations$ = this.results$.pipe(
    map((result) => result?.aggregationsByTags ?? {}),
    map((agg) =>
      Object.entries(agg)
        .sort(([, a], [, b]) => b - a)
        .map(([tag, count]) => ({ tag, count }))
    )
  );

  constructor(private readonly api: SearchApiService, private readonly fb: FormBuilder, private readonly cdr: ChangeDetectorRef) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onQuerySubmit(): void {
    const query = this.currentQuery();
    if (!query) {
      this.message$.next('Please enter a search term.');
      return;
    }

    this.activeFilters$.next(new Set());
    this.search(query, []);
  }

  onSuggestionSelected(term: string): void {
    this.searchForm.controls.query.setValue(term, { emitEvent: false });
    this.activeFilters$.next(new Set());
    this.search(term, []);
  }

  onAutocompleteSelected(term: string): void {
    const current = this.searchForm.controls.query.value ?? '';
    const parts = current.split(' ');
    if (parts.length === 0) {
      this.searchForm.controls.query.setValue(term);
      return;
    }
    parts[parts.length - 1] = term;
    this.searchForm.controls.query.setValue(parts.join(' '));
  }

  onFiltersChanged(tag: string): void {
    const next = new Set(this.activeFilters$.value);
    if (next.has(tag)) {
      next.delete(tag);
    } else {
      next.add(tag);
    }
    this.activeFilters$.next(next);

    const query = this.currentQuery();
    if (!query) {
      return;
    }

    this.search(query, Array.from(next));
  }

  onResultSelected(hit: SearchHit): void {
    this.api
      .getPost(hit.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (detail) => {
          this.selectedPost$.next(detail);
          this.cdr.markForCheck();
        },
        error: () => {
          this.message$.next('Could not load the selected post.');
          this.selectedPost$.next(null);
          this.cdr.markForCheck();
        }
      });

    this.api
      .moreLikeThis(hit.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (similar) => {
          this.similarPosts$.next(similar);
          this.cdr.markForCheck();
        },
        error: () => {
          this.similarPosts$.next([]);
          this.cdr.markForCheck();
        }
      });
  }

  private search(query: string, categories: string[]): void {
    this.isLoading$.next(true);
    this.message$.next('');
    this.results$.next(null);
    this.selectedPost$.next(null);
    this.similarPosts$.next([]);
    this.suggestedTerms$.next([]);

    const request$ = categories.length
      ? this.api.searchByCategories(query, categories)
      : this.api.search(query);

    request$
      .pipe(
        takeUntil(this.destroy$),
        catchError((error) => {
          console.error('Search failed', error);
          this.message$.next('We were unable to complete the search.');
          this.results$.next({ aggregationsByTags: {}, results: [], took: 0, total: 0 });
          return of({ aggregationsByTags: {}, results: [], took: 0, total: 0 });
        }),
        finalize(() => this.isLoading$.next(false))
      )
      .subscribe((result) => {
        this.results$.next(result);
        if (!result.results.length) {
          this.message$.next('No results found.');
        }
        this.fetchSuggestions(query);
        this.cdr.markForCheck();
      });
  }

  private fetchSuggestions(query: string): void {
    this.api
      .suggest(query)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (suggestions) => {
          this.suggestedTerms$.next(suggestions);
          this.cdr.markForCheck();
        },
        error: () => {
          this.suggestedTerms$.next([]);
          this.cdr.markForCheck();
        }
      });
  }

  private loadAutocomplete(value: string): Observable<string[]> {
    const query = value?.trim();
    if (!query) {
      return of([]);
    }

    const pieces = query.split(' ');
    const current = pieces[pieces.length - 1] ?? '';

    return this.api.autocomplete(current).pipe(
      catchError(() => of([]))
    );
  }

  private currentQuery(): string {
    return this.searchForm.controls.query.value?.trim() ?? '';
  }
}
