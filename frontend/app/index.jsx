import { useEffect, useState } from 'react'
import { Redirect } from 'expo-router'
import { getItem } from '../config/storage'
import { roleFromToken } from '../config/session'
import { Loading } from '../components/caregiver/ui'

export default function Home() {
    const [destination, setDestination] = useState(null)
    useEffect(() => {
        getItem('accessToken').then((token) => {
            const role = roleFromToken(token)
            setDestination(role === 'Person' ? '/(caregiver)/transport'
                : role === 'Association' ? '/association/request' : '/login')
        }).catch(() => setDestination('/login'))
    }, [])
    return destination ? <Redirect href={destination} /> : <Loading />
}
