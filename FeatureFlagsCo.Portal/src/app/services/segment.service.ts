import { environment } from 'src/environments/environment';
import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable, of } from "rxjs";
import {
  SegmentListFilter,
  SegmentListItem,
  SegmentListModel
} from "../pages/main/segments/types/segments-index";
import { getCurrentProjectEnv } from "../utils/project-env";
import { catchError } from "rxjs/operators";

@Injectable({
  providedIn: 'root'
})
export class SegmentService {

  public current: SegmentListItem = null;
  public envId: number;

  get baseUrl() {
    return `${environment.url}/api/v2/envs/${this.envId}/feature-flag`;
  }

  constructor(private http: HttpClient) {
    this.envId = getCurrentProjectEnv().envId;
  }

  getSegmentList(filter: SegmentListFilter = new SegmentListFilter()): Observable<SegmentListModel> {
    const queryParam = {
      name: filter.name ?? '',
      pageIndex: filter.pageIndex - 1,
      pageSize: filter.pageSize,
    };

    return this.http.get<SegmentListModel>(
      this.baseUrl,
      {params: new HttpParams({fromObject: queryParam})}
    );
  }

  isNameUsed(name: string): Observable<boolean> {
    const url = `${this.baseUrl}/is-name-used?name=${name}`;

    return this.http.get<boolean>(url).pipe(catchError(() => of(undefined)));
  }

  // 快速创建新的开关
  public create(name, description: string) {
    const url = environment.url + '/FeatureFlags/CreateFeatureFlag';
    const body = {
      name: name,
      description,
      environmentId: this.envId,
    };

    return this.http.post(url, body);
  }

  public setCurrent(data: SegmentListItem) {
    this.current = data;
  }
}
