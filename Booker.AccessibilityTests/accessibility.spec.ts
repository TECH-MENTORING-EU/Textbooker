import AxeBuilder from '@axe-core/playwright';
import { expect, Page, test } from '@playwright/test';

const wcagTags = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'];

async function expectNoWcagViolations(page: Page) {
  const results = await new AxeBuilder({ page }).withTags(wcagTags).analyze();
  expect(results.violations, JSON.stringify(results.violations, null, 2)).toEqual([]);
}

async function signIn(page: Page) {
  await page.goto('/Identity/Account/Login');
  await page.getByLabel('Nazwa użytkownika / e-mail:').fill('accessibility-user');
  await page.getByLabel('Hasło:').fill('Accessibility1!');
  await page.getByRole('button', { name: 'Zaloguj się' }).click();
  await expect(page).toHaveURL(/\/$/);
}

for (const path of ['/', '/Identity/Account/Login', '/Identity/Account/Register', '/Privacy', '/Chat', '/Sitemap']) {
  test(`${path} has no detectable WCAG A/AA violations`, async ({ page }) => {
    await page.goto(path);
    await expectNoWcagViolations(page);
  });
}

test('key public pages pass axe in dark color scheme', async ({ page }) => {
  await page.emulateMedia({ colorScheme: 'dark' });
  for (const path of ['/', '/Identity/Account/Login', '/Chat']) {
    await page.goto(path);
    await expectNoWcagViolations(page);
  }
});

test('authenticated account and admin pages have no detectable WCAG A/AA violations', async ({ page }) => {
  await signIn(page);
  const paths = [
    '/Add',
    '/Book/1',
    '/Edit/1',
    '/Profile',
    '/Profile/Favorites',
    '/Identity/Account/Manage',
    '/Identity/Account/Manage/Email',
    '/Identity/Account/Manage/ChangePassword',
    '/Identity/Account/Manage/PersonalData',
    '/Identity/Account/Manage/ProfilePictureUpload',
    '/Admin',
    '/Admin/Users',
    '/Admin/Schools',
    '/Admin/Admins'
  ];

  for (const path of paths) {
    await test.step(path, async () => {
      await page.goto(path);
      await expectNoWcagViolations(page);
    });
  }
});

test('seeded listing, details and edit views render accessible real content', async ({ page }) => {
  await page.goto('/');
  const listingLink = page.getByRole('link', { name: 'Ponad słowami 1 cz. 1' });
  await expect(listingLink).toBeVisible();
  await expectNoWcagViolations(page);

  await listingLink.click();
  await expect(page).toHaveURL(/\/Book\/1$/);
  await expect(page.getByRole('heading', { level: 1, name: 'Ponad słowami 1 cz. 1' })).toBeVisible();
  await expectNoWcagViolations(page);

  await signIn(page);
  await page.goto('/Edit/1');
  await expect(page.getByRole('heading', { level: 1, name: 'Edytuj ogłoszenie:' })).toBeVisible();
  await expectNoWcagViolations(page);
});

test('HTMX search updates results and announces loading without losing the layout', async ({ page }) => {
  await page.goto('/');
  const search = page.getByRole('searchbox', { name: 'Szukana fraza' });
  const results = page.getByRole('region', { name: 'Wyniki wyszukiwania' });

  await search.fill('nieistniejący podręcznik');
  await expect(results).toContainText('Brak wyników');
  await expectNoWcagViolations(page);

  await search.fill('');
  await search.press('Enter');
  await expect(results.getByRole('heading', { name: 'Ponad słowami 1 cz. 1' })).toBeVisible({ timeout: 10_000 });
  await expectNoWcagViolations(page);
});

test('HTMX announces failed result updates as errors', async ({ page }) => {
  await page.goto('/');
  const indicator = page.locator('#ind');

  await page.evaluate(() => {
    const source = document.querySelector('.filter-search');
    document.body.dispatchEvent(new CustomEvent('htmx:afterRequest', {
      bubbles: true,
      detail: { elt: source, successful: false }
    }));
  });

  await expect(indicator).toHaveText('Nie udało się zaktualizować wyników. Spróbuj ponownie.');
});

test('closing the add summary resets its submission state without JavaScript errors', async ({ page }) => {
  await signIn(page);
  await page.goto('/Add');
  const errors: string[] = [];
  page.on('pageerror', error => errors.push(error.message));

  await page.evaluate(() => {
    const form = document.querySelector<HTMLFormElement>('#add-form');
    const dialog = document.querySelector<HTMLDialogElement>('#summaryModal');
    if (!form || !dialog) throw new Error('Add summary controls are missing.');
    form.dataset.inSummary = 'true';
    dialog.showModal();
  });
  await page.getByRole('button', { name: 'Zamknij podsumowanie' }).click();

  await expect(page.locator('#summaryModal')).not.toBeVisible();
  await expect.poll(() => page.locator('#add-form').getAttribute('data-in-summary')).toBeNull();
  expect(errors).toEqual([]);
});

test('openDialog ignores invalid triggers and accepts a dialog element', async ({ page }) => {
  await page.goto('/');

  const result = await page.evaluate(() => {
    const invalidTrigger = document.createElement('button');
    document.body.appendChild(invalidTrigger);
    (window as typeof window & { openDialog: (target: HTMLElement) => void }).openDialog(invalidTrigger);

    const dialog = document.querySelector<HTMLDialogElement>('#modal-example');
    if (!dialog) throw new Error('Help dialog is missing.');
    (window as typeof window & { openDialog: (target: HTMLElement) => void }).openDialog(dialog);
    const opened = dialog.open;
    dialog.close();
    invalidTrigger.remove();
    return opened;
  });

  expect(result).toBe(true);
});

test('content images have valid sources and decode successfully', async ({ page }) => {
  for (const path of ['/', '/Book/1']) {
    await page.goto(path);
    const brokenImages = await page.locator('img').evaluateAll(images => images.flatMap(image => {
      const element = image as HTMLImageElement;
      return element.currentSrc && element.complete && element.naturalWidth > 0 ? [] : [element.getAttribute('src') ?? '(brak src)'];
    }));
    expect(brokenImages, `${path} contains broken images`).toEqual([]);
  }

  await signIn(page);
  for (const path of ['/Profile', '/Identity/Account/Manage/ProfilePictureUpload']) {
    await page.goto(path);
    await expect(page.locator('img')).not.toHaveCount(0);
    const brokenImages = await page.locator('img').evaluateAll(images => images.flatMap(image => {
      const element = image as HTMLImageElement;
      return element.currentSrc && element.complete && element.naturalWidth > 0 ? [] : [element.getAttribute('src') ?? '(brak src)'];
    }));
    expect(brokenImages, `${path} contains broken images`).toEqual([]);
  }
});

test('critical pages reflow without horizontal document scrolling at 320 CSS pixels', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 800 });
  const expectNoHorizontalOverflow = async (path: string) => {
    await page.goto(path);
    const dimensions = await page.evaluate(() => {
      const viewport = document.documentElement.clientWidth;
      return {
        viewport,
        content: document.documentElement.scrollWidth,
        overflowingElements: Array.from(document.querySelectorAll<HTMLElement>('body *')).flatMap(element => {
          const rect = element.getBoundingClientRect();
          if (rect.left >= -1 && rect.right <= viewport + 1) return [];
          const identifier = `${element.tagName.toLowerCase()}${element.id ? `#${element.id}` : ''}${
            typeof element.className === 'string' && element.className.trim()
              ? `.${element.className.trim().split(/\s+/).join('.')}`
              : ''
          }`;
          return [`${identifier}: left=${rect.left.toFixed(1)}, right=${rect.right.toFixed(1)}, width=${rect.width.toFixed(1)}`];
        }).slice(0, 10)
      };
    });
    expect(
      dimensions.content,
      `${path} overflows horizontally: ${dimensions.overflowingElements.join('; ')}`
    ).toBeLessThanOrEqual(dimensions.viewport + 1);
  };

  for (const path of ['/', '/Identity/Account/Login', '/Identity/Account/Register', '/Chat', '/Sitemap']) {
    await expectNoHorizontalOverflow(path);
  }
  await signIn(page);
  for (const path of ['/Add', '/Book/1', '/Edit/1', '/Profile', '/Identity/Account/Manage/ProfilePictureUpload', '/Admin/Users']) {
    await expectNoHorizontalOverflow(path);
  }
});

test('visible controls meet the 24 CSS pixel minimum target size', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 800 });
  const expectMinimumTargetSize = async (path: string) => {
    await page.goto(path);
    const undersized = await page.locator('button, input:not([type="hidden"]), select, textarea, summary, a[role="button"]').evaluateAll(elements =>
      elements.flatMap(element => {
        const rect = element.getBoundingClientRect();
        const style = getComputedStyle(element);
        if (style.display === 'none' || style.visibility === 'hidden' || rect.width === 0 || rect.height === 0) return [];
        return rect.width < 24 || rect.height < 24
          ? [`${element.tagName.toLowerCase()}#${element.id}.${element.className}: ${rect.width.toFixed(1)}x${rect.height.toFixed(1)}`]
          : [];
      })
    );
    expect(undersized, `${path} has undersized controls`).toEqual([]);
  };

  for (const path of ['/', '/Identity/Account/Login', '/Identity/Account/Register', '/Chat', '/Sitemap']) {
    await expectMinimumTargetSize(path);
  }
  await signIn(page);
  for (const path of ['/Add', '/Book/1', '/Edit/1', '/Profile', '/Identity/Account/Manage/ProfilePictureUpload', '/Admin/Users']) {
    await expectMinimumTargetSize(path);
  }
});

test('skip link moves keyboard focus to the main content', async ({ page }) => {
  await page.goto('/');
  await page.keyboard.press('Tab');
  const skipLink = page.getByRole('link', { name: 'Przejdź do treści głównej' });
  await expect(skipLink).toBeFocused();
  await skipLink.press('Enter');
  await expect(page.locator('#main-content')).toBeFocused();
});

test('profile cropper exposes named non-drag and keyboard controls', async ({ page }) => {
  await signIn(page);
  await page.goto('/Identity/Account/Manage/ProfilePictureUpload');
  await page.getByRole('button', { name: 'Zmień zdjęcie profilowe' }).click();

  const dialog = page.getByRole('dialog', { name: 'Prześlij i dostosuj nowe zdjęcie' });
  await expect(dialog).toBeVisible();

  await page.locator('#imageInput').setInputFiles({
    name: 'profile.png',
    mimeType: 'image/png',
    buffer: Buffer.from(
      'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=',
      'base64'
    )
  });

  await expect(dialog.getByRole('group', { name: 'Przesuwanie zdjęcia' })).toBeVisible();
  await expect(dialog.getByRole('button', { name: 'Przesuń zdjęcie w górę' })).toBeVisible();
  const canvas = dialog.locator('#cropperCanvas');
  await canvas.focus();
  await canvas.press('ArrowRight');
  await expect(canvas).toBeFocused();
  await expectNoWcagViolations(page);

  await dialog.getByRole('button', { name: 'Zamknij edycję zdjęcia' }).click();
  await expect(page.getByRole('button', { name: 'Zmień zdjęcie profilowe' })).toBeFocused();
});
