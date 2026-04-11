/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-02-04  Greeley                 Gets or creates a user object

import { createUser } from './api-requests.js'

/**
 * Gets the exisiting user from the Local Storage or creates a new one with a random name
 * from our user name json. If we're create a new user, we're also gonna make a POST
 * to the backend with that users object.
 * @returns
 */
export async function getOrCreateUser() {
    const storedUser = localStorage.getItem('ameni_user')
    console.log('username: ', storedUser)
    if (storedUser) return JSON.parse(storedUser)

    const response = await fetch('/Scripts/usernames.json')

    const names = await response.json()
    console.log('username: ', names)
    const randomName = names[Math.floor(Math.random() * names.length)];
    const user = await createUser(randomName)

    localStorage.setItem('ameni_user', JSON.stringify(user))
    console.log('username: ', user)

    return user
}