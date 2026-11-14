// Il ruolo serve alla navigazione; l'autorizzazione è sempre verificata dal server.
export const roleFromToken = (token) => {
    try {
        const part = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
        const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'
        let bits = 0
        let value = 0
        let json = ''
        for (const ch of part.replace(/=+$/, '')) {
            value = (value << 6) | chars.indexOf(ch)
            bits += 6
            if (bits >= 8) {
                bits -= 8
                json += String.fromCharCode((value >> bits) & 0xff)
            }
        }
        return JSON.parse(json).role
    } catch {
        return null
    }
}
