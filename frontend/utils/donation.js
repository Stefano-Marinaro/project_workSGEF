// Keep invalid input out of currency formatting and use integer cents for the total.
export const MAX_DONATION_DIGITS = 10
export const donationDigitCount = (raw) => (String(raw ?? '').match(/[0-9]/g) || []).length

export const donationEstimate = (raw, enabled, minimum) => {
    const baseCents = Number.isFinite(minimum) ? Math.round(Math.max(minimum, 0) * 100) : 0
    const safeBase = Number.isSafeInteger(baseCents) ? baseCents : 0
    const result = { extra: 0, total: safeBase / 100, error: '' }
    if (!enabled) return result

    const text = String(raw ?? '').trim()
    if (!text) return { ...result, error: 'Inserisci l’importo della donazione.' }
    if (donationDigitCount(text) > MAX_DONATION_DIGITS) {
        return { ...result, error: 'La donazione può contenere al massimo 10 cifre complessive.' }
    }
    if (text.length > 24) return { ...result, error: 'Importo troppo elevato.' }
    if (!/^(?:\d+(?:[.,]\d{0,2})?|[.,]\d{1,2})$/.test(text)) {
        return { ...result, error: 'Inserisci un importo valido, con al massimo due decimali (es. 10,50).' }
    }

    const [whole, fraction = ''] = text.replace(',', '.').split('.')
    const cents = Number(whole || '0') * 100 + Number(fraction.padEnd(2, '0'))
    if (!Number.isSafeInteger(cents) || !Number.isSafeInteger(safeBase + cents)) {
        return { ...result, error: 'Importo troppo elevato.' }
    }
    if (cents <= 0) return { ...result, error: 'Inserisci un importo maggiore di zero.' }
    return { extra: cents / 100, total: (safeBase + cents) / 100, error: '' }
}
