import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs/operators';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-user-detail',
  templateUrl: './user-detail.component.html',
  styleUrls: ['./user-detail.component.less']
})
export class UserDetailComponent implements OnInit {

  userId: string;

  defaultKeys = ['keyId', 'name', 'email'];

  propertyList = [];

  constructor(
    private route: ActivatedRoute
  ) {
   }

  ngOnInit(): void {
    this.listenerResolveData();
  }

  listenerResolveData() {
    this.route.data
      .pipe(
        map(res => res.userDetail)
      )
      .subscribe(res => {
        const user = res;
        if (res) {
          if (!res.customizedProperties) {
            this.propertyList = this.defaultKeys.map(key => ({
              name: key,
              value: user[key]
            }));
          } else {
            this.propertyList = [...user.customizedProperties,
              ...this.defaultKeys.map(key => ({
                name: key,
                value: user[key]
              }))
            ];
          }
        }
      });
  }
}
