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

  envId: number;

  constructor(private http: HttpClient) {
    this.envId = getCurrentProjectEnv().envId;
  }

  getSwitchDropDown(): Observable<SwitchDropdown[]> {
    const url = `${environment.url}/api/v2/envs/${this.envId}/feature-flag/dropdown`;

    return this.http.get<SwitchDropdown[]>(url);
  }

  getSwitchList(filter: SwitchListFilter = new SwitchListFilter()): Observable<SwitchListModel> {
    const url = `${environment.url}/api/v2/envs/${this.envId}/feature-flag/`;

    const queryParam = {
      name: filter.name ?? '',
      status: filter.status ?? '',
      tagIds: filter.tagIds ?? [],
      pageIndex: filter.pageIndex - 1,
      pageSize: filter.pageSize,
    };

    return this.http.get<SwitchListModel>(
      url,
      {params: new HttpParams({fromObject: queryParam})}
    );
  }

  isNameUsed(name: string): Observable<boolean> {
    const url = `${environment.url}/api/v2/envs/${this.envId}/feature-flag/is-name-used?name=${name}`;

    return this.http.get<boolean>(url).pipe(catchError(() => of(undefined)));
  }
}
