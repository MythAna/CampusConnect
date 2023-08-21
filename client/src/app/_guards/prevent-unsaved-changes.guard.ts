import { CanDeactivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';
import { ConfirmService } from '../_services/confirm.service';


export const preventUnsavedChangesCuard: CanDeactivateFn<MemberEditComponent> = (component) => {
  const confirmService = inject (ConfirmService);

  if (component.editForm?.dirty) { 
    return confirmService.confirm();
  }

    return true;
};
