import { environment } from 'src/environments/environment';
import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import {
  SwitchListFilter,
  SwitchDropdown,
  SwitchListModel
} from "../pages/main/switch-manage/types/switch-index";

@Injectable({
  providedIn: 'root'
})
export class SwitchV2Service {

  constructor(private http: HttpClient) {
  }

  getSwitchDropDown(envId: number): Observable<SwitchDropdown[]> {
    const url = `${environment.url}/api/v2/envs/${envId}/feature-flag/dropdown`;

    return this.http.get<SwitchDropdown[]>(url);
  }

  getSwitchList(envId: number, filter: SwitchListFilter = new SwitchListFilter()): Observable<SwitchListModel> {
    const url = `${environment.url}/api/v2/envs/${envId}/feature-flag/`;

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
}
