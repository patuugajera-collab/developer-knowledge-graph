import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';

export const DATABASE_DOWN_MESSAGE = 'Unable to connect to the database. Please try again.';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      toast.error(toUserMessage(error));
      return throwError(() => error);
    }),
  );
};

function toUserMessage(error: HttpErrorResponse): string {
  if (error.status === 0) {
    return 'Unable to reach the API server. Please try again.';
  }

  if (error.status === 503) {
    return DATABASE_DOWN_MESSAGE;
  }

  const body = error.error as { message?: string } | null;
  if (body?.message) {
    return body.message;
  }

  return error.status >= 500
    ? 'An unexpected error occurred. Please try again later.'
    : `Request failed (${error.status}). Please try again.`;
}