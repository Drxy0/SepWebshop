import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, map, shareReplay } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TranslationService {
  private currentLang = 'en';
  private translations = new BehaviorSubject<Record<string, string>>({});
  private loadedTranslations: Record<string, Observable<Record<string, string>>> = {};

  constructor(private http: HttpClient) {
    this.loadLanguage('en');
  }

  private loadLanguage(lang: string): void {
    if (!this.loadedTranslations[lang]) {
      this.loadedTranslations[lang] = this.http
        .get<Record<string, string>>(`/i18n/${lang}.json`)
        .pipe(shareReplay(1));
    }

    this.loadedTranslations[lang].subscribe(translations => {
      this.translations.next(translations);
    });
  }

  setLanguage(lang: string): void {
    this.currentLang = lang;
    this.loadLanguage(lang);
  }

  getCurrentLanguage(): string {
    return this.currentLang;
  }

  // Get translation as observable (reactive)
  translate(key: string): Observable<string> {
    return this.translations.pipe(
      map(translations => translations[key] || key)
    );
  }

  // Get translation synchronously (for templates)
  t(key: string): string {
    const translations = this.translations.getValue();
    return translations[key] || key;
  }
}