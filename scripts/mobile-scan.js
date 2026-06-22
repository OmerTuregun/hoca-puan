/**
 * HocaPuan frontend — mobil responsive tarama (geliştirme aracı, production'a dahil değil).
 *
 * Kullanım:
 *   cd scripts && npm install && npm run mobile-scan
 *
 * Ortam değişkenleri:
 *   FRONTEND_URL  — varsayılan http://127.0.0.1:8089
 *   ADMIN_EMAIL   — varsayılan admin@hocapuan.com
 *   ADMIN_PASSWORD — varsayılan Admin123!
 */

import { chromium } from 'playwright'
import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const OUTPUT_DIR = path.join(__dirname, 'output', 'mobile-screenshots')
const REPORT_PATH = path.join(__dirname, 'output', 'mobile-scan-report.json')

const BASE_URL = (process.env.FRONTEND_URL || 'http://127.0.0.1:8089').replace(/\/$/, '')
const ADMIN_EMAIL = process.env.ADMIN_EMAIL || 'admin@hocapuan.com'
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD || 'Admin123!'

const VIEWPORTS = [
  { width: 375, label: '375px', device: 'iPhone SE' },
  { width: 390, label: '390px', device: 'iPhone 12/13/14' },
  { width: 768, label: '768px', device: 'tablet' },
]

const VIEWPORT_HEIGHT = 844

/** @type {import('playwright').Page} */
let page

/** @type {Array<Record<string, unknown>>} */
const report = {
  scannedAt: new Date().toISOString(),
  baseUrl: BASE_URL,
  viewports: VIEWPORTS.map(v => ({ width: v.width, label: v.label, device: v.device })),
  professorId: null,
  professorNote: null,
  pages: [],
  summary: [],
}

function slug(name) {
  return name
    .toLowerCase()
    .replace(/ğ/g, 'g').replace(/ü/g, 'u').replace(/ş/g, 's')
    .replace(/ı/g, 'i').replace(/ö/g, 'o').replace(/ç/g, 'c')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-|-$/g, '')
}

async function ensureDirs() {
  fs.mkdirSync(OUTPUT_DIR, { recursive: true })
}

async function discoverProfessorId(request) {
  try {
    const res = await request.get(`${BASE_URL}/api/professors?page=1&pageSize=1`)
    if (!res.ok()) return null
    const data = await res.json()
    const id = data?.items?.[0]?.id
    return id ?? null
  } catch {
    return null
  }
}

async function runDomChecks() {
  return page.evaluate(() => {
    /** @type {Array<Record<string, unknown>>} */
    const issues = []

    const scrollWidth = Math.max(
      document.documentElement.scrollWidth,
      document.body?.scrollWidth ?? 0,
    )
    const innerWidth = window.innerWidth
    if (scrollWidth > innerWidth + 1) {
      issues.push({
        type: 'horizontal-scroll',
        scrollWidth,
        innerWidth,
        overflowPx: scrollWidth - innerWidth,
      })
    }

    const interactiveSelector =
      'a, button, [role="button"], input[type="submit"], input[type="button"], summary'
    /** @type {Array<Record<string, unknown>>} */
    const smallTargets = []

    document.querySelectorAll(interactiveSelector).forEach((el) => {
      const style = window.getComputedStyle(el)
      if (style.display === 'none' || style.visibility === 'hidden') return
      if (style.pointerEvents === 'none') return
      const rect = el.getBoundingClientRect()
      if (rect.width < 1 || rect.height < 1) return
      if (rect.bottom < 0 || rect.top > window.innerHeight) return

      if (rect.width < 44 || rect.height < 44) {
        const text = (el.textContent || el.getAttribute('aria-label') || '').trim().slice(0, 80)
        smallTargets.push({
          tag: el.tagName.toLowerCase(),
          text,
          width: Math.round(rect.width),
          height: Math.round(rect.height),
          hint: el.id ? `#${el.id}` : (typeof el.className === 'string' ? el.className.split(/\s+/).slice(0, 2).join('.') : ''),
        })
      }
    })

    if (smallTargets.length > 0) {
      issues.push({
        type: 'small-touch-target',
        count: smallTargets.length,
        elements: smallTargets.slice(0, 20),
      })
    }

    const textSelector = 'p, span, a, button, label, h1, h2, h3, h4, h5, h6, li, td, th, small, div'
    /** @type {Array<Record<string, unknown>>} */
    const smallFonts = []
    const seen = new Set()

    document.querySelectorAll(textSelector).forEach((el) => {
      const style = window.getComputedStyle(el)
      if (style.display === 'none' || style.visibility === 'hidden') return
      const fontSize = parseFloat(style.fontSize)
      if (!(fontSize > 0 && fontSize < 12)) return

      const text = (el.textContent || '').replace(/\s+/g, ' ').trim()
      if (text.length < 2) return
      const key = `${el.tagName}:${text.slice(0, 40)}:${fontSize}`
      if (seen.has(key)) return
      seen.add(key)

      smallFonts.push({
        tag: el.tagName.toLowerCase(),
        fontSizePx: fontSize,
        text: text.slice(0, 80),
      })
    })

    if (smallFonts.length > 0) {
      issues.push({
        type: 'small-font',
        count: smallFonts.length,
        elements: smallFonts.slice(0, 20),
      })
    }

    return issues
  })
}

async function capturePage(pageKey, pageLabel, url, options = {}) {
  const { waitFor, beforeScreenshot, requiresAuth, note } = options
  const entry = {
    key: pageKey,
    label: pageLabel,
    url,
    note: note ?? null,
    viewports: [],
  }

  for (const vp of VIEWPORTS) {
    await page.setViewportSize({ width: vp.width, height: VIEWPORT_HEIGHT })
    await page.goto(url, { waitUntil: 'networkidle', timeout: 30000 })
    if (waitFor) await waitFor()
    if (beforeScreenshot) await beforeScreenshot()

    await page.waitForTimeout(600)

    const screenshotName = `${slug(pageKey)}-${vp.label}.png`
    const screenshotPath = path.join(OUTPUT_DIR, screenshotName)
    await page.screenshot({ path: screenshotPath, fullPage: true })

    const issues = await runDomChecks()
    const vpEntry = {
      viewport: vp.label,
      width: vp.width,
      screenshot: path.relative(path.join(__dirname, '..'), screenshotPath),
      issues,
    }
    entry.viewports.push(vpEntry)

    for (const issue of issues) {
      report.summary.push({
        page: pageLabel,
        pageKey,
        viewport: vp.label,
        issueType: issue.type,
        detail: formatIssueDetail(issue),
      })
    }
  }

  report.pages.push(entry)
}

function formatIssueDetail(issue) {
  switch (issue.type) {
    case 'horizontal-scroll':
      return `Yatay scroll: scrollWidth=${issue.scrollWidth}, innerWidth=${issue.innerWidth} (+${issue.overflowPx}px taşma)`
    case 'small-touch-target':
      return `${issue.count} tıklanabilir eleman 44×44px altında (ör. ${describeElement(issue.elements?.[0])})`
    case 'small-font':
      return `${issue.count} metin elemanı 12px altında (ör. ${describeFont(issue.elements?.[0])})`
    default:
      return JSON.stringify(issue)
  }
}

function describeElement(el) {
  if (!el) return '—'
  const text = el.text ? `"${el.text}"` : el.hint || el.tag
  return `${el.tag} ${text} (${el.width}×${el.height}px)`
}

function describeFont(el) {
  if (!el) return '—'
  return `${el.tag} ${el.fontSizePx}px "${el.text}"`
}

async function loginAsAdmin() {
  await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' })
  await page.fill('input[type="email"]', ADMIN_EMAIL)
  await page.fill('input[type="password"]', ADMIN_PASSWORD)
  await page.click('button[type="submit"]')
  await page.waitForTimeout(2000)
}

async function fillReviewForm() {
  const qualityStars = page.locator('[aria-label="4 yıldız"]')
  await qualityStars.nth(0).click()
  await qualityStars.nth(1).click()

  await page.fill('textarea', 'Mobil tarama test yorumu — ders anlatımı ve sınav deneyimi hakkında örnek metin.')

  const tags = page.locator('button[type="button"]').filter({ hasText: /^[A-ZÇĞİÖŞÜ]/ })
  if (await tags.count() > 0) {
    await tags.first().click().catch(() => {})
  }
}

async function captureReviewOverlay(professorId) {
  const pageKey = 'yorum-formu-overlay'
  const pageLabel = 'Yorum formu (overlay açık)'
  const url = `${BASE_URL}/professors/${professorId}/review`
  const entry = {
    key: pageKey,
    label: pageLabel,
    url,
    note: 'Overlay, gönderim sırasında API geciktirilerek yakalandı',
    viewports: [],
  }

  for (const vp of VIEWPORTS) {
    await page.setViewportSize({ width: vp.width, height: VIEWPORT_HEIGHT })

    await page.route('**/api/reviews', async (route) => {
      if (route.request().method() === 'POST') {
        await new Promise(r => setTimeout(r, 15000))
        await route.abort('failed')
      } else {
        await route.continue()
      }
    })

    try {
      await page.goto(url, { waitUntil: 'networkidle', timeout: 30000 })
      await page.waitForSelector('h1:has-text("Yorum yaz")', { timeout: 10000 })
      await fillReviewForm()

      const submitBtn = page.getByRole('button', { name: /Yorumu gönder|Gönderiliyor/i })
      await submitBtn.click()

      await page.waitForSelector('[role="alertdialog"]', { timeout: 8000 })
      await page.waitForTimeout(400)

      const screenshotName = `${slug(pageKey)}-${vp.label}.png`
      const screenshotPath = path.join(OUTPUT_DIR, screenshotName)
      await page.screenshot({ path: screenshotPath, fullPage: false })

      const issues = await runDomChecks()
      entry.viewports.push({
        viewport: vp.label,
        width: vp.width,
        screenshot: path.relative(path.join(__dirname, '..'), screenshotPath),
        issues,
      })

      for (const issue of issues) {
        report.summary.push({
          page: pageLabel,
          pageKey,
          viewport: vp.label,
          issueType: issue.type,
          detail: formatIssueDetail(issue),
        })
      }
    } catch (err) {
      entry.viewports.push({
        viewport: vp.label,
        width: vp.width,
        screenshot: null,
        error: String(err),
        issues: [],
      })
      report.summary.push({
        page: pageLabel,
        pageKey,
        viewport: vp.label,
        issueType: 'capture-error',
        detail: `Overlay ekran görüntüsü alınamadı: ${err}`,
      })
    } finally {
      await page.unroute('**/api/reviews').catch(() => {})
    }
  }

  report.pages.push(entry)
}

async function main() {
  await ensureDirs()

  const browser = await chromium.launch({ headless: true })
  const context = await browser.newContext({
    locale: 'tr-TR',
    ignoreHTTPSErrors: true,
  })
  page = await context.newPage()

  const professorId = await discoverProfessorId(context.request)
  report.professorId = professorId
  if (!professorId) {
    report.professorNote =
      'Veritabanında hoca kaydı bulunamadı; hoca profili ve yorum formu boş/hata durumuyla tarandı.'
  }

  const professorUrl = professorId
    ? `${BASE_URL}/professors/${professorId}`
    : `${BASE_URL}/professors/1`

  // Herkese açık sayfalar
  await capturePage('anasayfa', 'Ana sayfa', `${BASE_URL}/`)
  await capturePage('hoca-profili', 'Hoca profili', professorUrl, {
    note: professorId ? null : 'ID=1 placeholder; kayıt yoksa "Hoca bulunamadı" görünür',
  })
  await capturePage('arama', 'Arama / sonuç', `${BASE_URL}/search?q=matematik`)
  await capturePage('login', 'Login', `${BASE_URL}/login`)
  await capturePage('register', 'Register', `${BASE_URL}/register`)

  // Auth gerektiren sayfalar
  await loginAsAdmin()

  await capturePage('profil', 'Profil', `${BASE_URL}/profile`, { requiresAuth: true })
  await capturePage('admin-moderasyon', 'Admin moderasyon', `${BASE_URL}/admin/moderation`, {
    requiresAuth: true,
    waitFor: async () => {
      await page.waitForTimeout(1500)
    },
  })

  const reviewUrl = professorId
    ? `${BASE_URL}/professors/${professorId}/review`
    : `${BASE_URL}/professors/1/review`

  await capturePage('yorum-formu', 'Yorum yazma formu', reviewUrl, {
    requiresAuth: true,
    note: professorId ? null : 'Hoca olmadığı için form yüklenmeyebilir',
    waitFor: async () => {
      await page.waitForTimeout(1500)
    },
  })

  if (professorId) {
    await captureReviewOverlay(professorId)
  } else {
    report.pages.push({
      key: 'yorum-formu-overlay',
      label: 'Yorum formu (overlay açık)',
      url: reviewUrl,
      note: 'Atlandı — veritabanında hoca yok, overlay tetiklenemedi',
      viewports: [],
    })
    report.summary.push({
      page: 'Yorum formu (overlay açık)',
      pageKey: 'yorum-formu-overlay',
      viewport: '—',
      issueType: 'skipped',
      detail: 'Veritabanında hoca olmadığı için overlay ekran görüntüsü alınamadı',
    })
  }

  report.issueCount = report.summary.length
  report.issueCountByType = report.summary.reduce((acc, row) => {
    acc[row.issueType] = (acc[row.issueType] || 0) + 1
    return acc
  }, {})

  fs.writeFileSync(REPORT_PATH, JSON.stringify(report, null, 2), 'utf8')

  await browser.close()

  console.log(`\nTarama tamamlandı.`)
  console.log(`Rapor: ${REPORT_PATH}`)
  console.log(`Screenshot'lar: ${OUTPUT_DIR}`)
  console.log(`Toplam bulgu: ${report.summary.length}`)
}

main().catch((err) => {
  console.error(err)
  process.exit(1)
})
