import { Image, useColorScheme } from 'react-native'

// images 
import DarkLogo from '../assets/img/ivoDark.jpg'
import LightLogo from '../assets/img/ivoLight.jpg'

const ThemedLogo = ({ ...props }) => {
    const colorScheme = useColorScheme()

    const logo = colorScheme === 'dark' ? DarkLogo : LightLogo

    return (
        <Image source={logo} {...props} />
    )
}

export default ThemedLogo