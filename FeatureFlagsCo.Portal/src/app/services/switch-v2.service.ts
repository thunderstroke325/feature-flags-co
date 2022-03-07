import { environment } from 'src/environments/environment';
import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable, of } from "rxjs";
import {
  SwitchListFilter,
  SwitchDropdown,
  SwitchListModel
} from "../pages/main/switch-manage/types/switch-index";
import { getCurrentProjectEnv } from "../utils/project-env";
import { catchError } from "rxjs/operators";

@Injectable({
  providedIn: 'root'
})
export class SwitchV2Service {

  get baseUrl() {
    const envId = getCurrentProjectEnv().envId;
    return `${environment.url}/api/v2/envs/${envId}/feature-flag`;
  }

  constructor(private http: HttpClient) { }

  getSwitchDropDown(): Observable<SwitchDropdown[]> {
    const url = `${this.baseUrl}/dropdown`;

    return this.http.get<SwitchDropdown[]>(url);
  }

  getSwitchList(filter: SwitchListFilter = new SwitchListFilter()): Observable<SwitchListModel> {
    const queryParam = {
      name: filter.name ?? '',
      status: filter.status ?? '',
      tagIds: filter.tagIds ?? [],
      pageIndex: filter.pageIndex - 1,
      pageSize: filter.pageSize,
    };

    return this.http.get<SwitchListModel>(
      this.baseUrl,
      {params: new HttpParams({fromObject: queryParam})}
    );
  }

  isNameUsed(name: string): Observable<boolean> {
    const url = `${this.baseUrl}/is-name-used?name=${name}`;

    return this.http.get<boolean>(url).pipe(catchError(() => of(undefined)));
  }
}
