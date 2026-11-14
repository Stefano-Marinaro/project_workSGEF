import { createElement } from 'react'
import { C } from '../constants/caregiver'

// Original Material Design "history" geometry, matching the previously selected icon.
// Source: https://github.com/Templarian/MaterialDesign/blob/master/svg/history.svg
const HISTORY_PATH = 'M13.5,8H12V13L16.28,15.54L17,14.33L13.5,12.25V8M13,3A9,9 0 0,0 4,12H1L4.96,16.03L9,12H6A7,7 0 0,1 13,5A7,7 0 0,1 20,12A7,7 0 0,1 13,19C11.07,19 9.32,18.21 8.06,16.94L6.64,18.36C8.27,20 10.5,21 13,21A9,9 0 0,0 22,12A9,9 0 0,0 13,3'

export default function HistoryIcon({ size = 24, color = C.primary, backgroundColor = C.card }) {
    return createElement('svg', {
        width: size, height: size, viewBox: '0 0 24 24',
        'aria-hidden': true, focusable: false, style: { flexShrink: 0 },
    }, createElement('path', {
        d: HISTORY_PATH, fill: color,
        // Trim the filled glyph's edges without replacing its circular arrow or hands.
        stroke: backgroundColor, strokeWidth: 0.45, strokeLinejoin: 'round',
    }))
}
