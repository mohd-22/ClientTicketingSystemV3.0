import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const ageRangeValidator = (min: number, max: number): ValidatorFn => {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (!value) return null; 
    const inputDate = new Date(value);
    if (isNaN(inputDate.getTime())) {
      return { invalidDate: true };
    }
    const today = new Date();
    if (inputDate >= today) {
      return { futureDate: true };
    }
    let age = today.getFullYear() - inputDate.getFullYear();
    const m = today.getMonth() - inputDate.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < inputDate.getDate())) {
      age--;
    }
    if (age < min) return { tooYoung: { requiredAge: min, actual: age } };
    return null;
  };
};
