import { expect, test } from '@playwright/test';

test('accessibility test server is reachable', async ({ page }) => {
  const response = await page.goto('/');

  expect(response?.ok()).toBeTruthy();
  await expect(page.locator('body')).toBeVisible();
});
