/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-02-04  Greeley                 get the local stored user name or create and store it.

import { createUser } from './api-requests.js'

export async function getOrCreateUser() {
    const storedUser = localStorage.getItem('ameni_user')
    console.log('username: ', storedUser)
    if (storedUser) return JSON.parse(storedUser)

    const response = await fetch('./src/usernames.json')

    const names = await response.json()
    console.log('username: ', names)
    const randomName = names[Math.floor(Math.random() * names.length)];
    const user = await createUser(randomName)

    localStorage.setItem('ameni_user', JSON.stringify(user))
    console.log('username: ', user)

    return user
}