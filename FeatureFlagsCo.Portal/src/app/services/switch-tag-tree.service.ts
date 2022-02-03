import { environment } from 'src/environments/environment';
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { SwitchTagTree } from "../pages/main/switch-manage/types/switch-index";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";

@Injectable({
  providedIn: 'root'
})
export class SwitchTagTreeService {

  constructor(private http: HttpClient) {
  }

  // get switch tag tree
  getTree(envId: number): Observable<SwitchTagTree> {
    const url = `${environment.url}/api/v1/envs/${envId}/feature-flag-tag-tree`;

    return this.http.get(url).pipe(
      map((res: any) => new SwitchTagTree(res.trees))
    );
  }

  // save switch tag tree
  saveTree(envId: number, tree: SwitchTagTree): Observable<SwitchTagTree> {
    const url = `${environment.url}/api/v1/envs/${envId}/feature-flag-tag-tree`;

    return this.http.post(url, tree.trees).pipe(
      map((res: any) => new SwitchTagTree(res.trees))
    );
  }
}
