export interface SegmentListModel {
  items: SegmentListItem[];
  totalCount: number;
}

export interface SegmentListItem {
  id: string;
  name: string;
  description: string;
  lastModificationTime: Date
}

export class SegmentListFilter {
  name?: string;
  status?: string;
  pageIndex: number;
  pageSize: number;

  constructor(
    name?: string,
    status?: string,
    tagIds?: number[],
    pageIndex: number = 1,
    pageSize: number = 10) {
    this.name = name ?? '';
    this.status = status ?? '';
    this.pageIndex = pageIndex;
    this.pageSize = pageSize;
  }
}
