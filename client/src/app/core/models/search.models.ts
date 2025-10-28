export interface SearchHitDto {
  Id: string;
  Title: string;
  Body: string;
  Tags: string[];
  Score: number;
  AnswerCount: number;
}

export interface SearchResponseDto {
  Total: number;
  Results: SearchHitDto[];
  AggregationsByTags: Record<string, number>;
  ElapsedMilliseconds: number;
}

export interface PostDetailDto extends SearchHitDto {
  CreationDate?: string;
  Similar?: SearchHitDto[];
}

export interface SearchHit {
  id: string;
  title: string;
  body: string;
  tags: string[];
  score: number;
  answerCount: number;
}

export interface SearchResult {
  total: number;
  took: number;
  aggregationsByTags: Record<string, number>;
  results: SearchHit[];
}

export interface PostDetail extends SearchHit {
  creationDate?: string;
  similar?: SearchHit[];
}
