import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { SidebarComponent } from './sidebar.component';

describe('SidebarComponent', () => {
  let component: SidebarComponent;
  let fixture: ComponentFixture<SidebarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SidebarComponent, RouterTestingModule]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display admin name', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.admin-info h3')?.textContent).toContain('Admin User');
  });

  it('should have three navigation links', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const navLinks = compiled.querySelectorAll('.nav-link');
    expect(navLinks.length).toBe(3);
  });

  it('should have correct navigation text', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const navTexts = compiled.querySelectorAll('.nav-text');
    expect(navTexts[0].textContent).toContain('Patches');
    expect(navTexts[1].textContent).toContain('Customers');
    expect(navTexts[2].textContent).toContain('Status');
  });
});