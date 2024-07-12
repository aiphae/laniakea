import telebot
import requests
from telebot.types import ReplyKeyboardMarkup, KeyboardButton
from datetime import datetime
from io import BytesIO

API_TOKEN = ''
API_URL = ''

bot = telebot.TeleBot(API_TOKEN)

ERROR_MESSAGES = {
    "generic": "Something went wrong. Please try again.",
    "coordinates_not_set": "You need to set your coordinates first!\n\nUse /setloc command to update your location.",
    "invalid_date_format": "Invalid date format. Please use 'day.month.year'"
}


def prompt_for_coordinates(message):
    markup = ReplyKeyboardMarkup(one_time_keyboard=True, resize_keyboard=True)
    location_button = KeyboardButton('Send Location', request_location=True)
    markup.add(location_button)
    bot.register_next_step_handler(message, receive_coordinates)


def receive_coordinates(message):
    user_id = message.from_user.id
    if message.content_type == 'location':
        latitude, longitude = message.location.latitude, message.location.longitude
    else:
        try:
            latitude, longitude = map(float, message.text.split())
        except ValueError:
            bot.send_message(message.chat.id, "Invalid format. Please provide coordinates in 'latitude longitude' format.")
            prompt_for_coordinates(message)
            return

    data = {'id': str(user_id), 'latitude': latitude, 'longitude': longitude}
    response = requests.post(f"{API_URL}/DynamoDb", json=data)
    if response.status_code != 200:
        bot.send_message(message.chat.id, ERROR_MESSAGES["generic"])
        return

    bot.send_message(message.chat.id, "Coordinates have been set successfully!")


def get_user_coordinates(user_id):
    response = requests.get(f"{API_URL}/DynamoDb", params={'id': str(user_id)})
    if response.status_code != 200:
        return None
    data = response.json()
    return data['latitude'], data['longitude']


@bot.message_handler(commands=['start'])
def send_welcome(message):
    bot.send_message(message.chat.id, "🌠 Welcome to the Laniakea bot!\n\n"
                                      "To enhance your experience, please send your location or "
                                      "latitude and longitude separated by a space.")
    prompt_for_coordinates(message)


@bot.message_handler(commands=['setloc'])
def set_location(message):
    bot.send_message(message.chat.id, "Please send your new location or latitude and longitude separated by space.")
    prompt_for_coordinates(message)


@bot.message_handler(commands=['clearloc'])
def clear_location(message):
    response = requests.delete(f"{API_URL}/DynamoDb", params={'id': message.chat.id})
    if response.status_code != 200:
        bot.send_message(message.chat.id, ERROR_MESSAGES["generic"])
        return
    bot.send_message(message.chat.id, "Your coordinates have been deleted from the database.")


@bot.message_handler(commands=['apod'])
def get_apod(message):
    args = message.text.split()[1:]
    date = datetime.now().strftime('%Y-%m-%d') if not args else parse_date(args[0], message)
    if not date:
        return

    response = requests.get(f"{API_URL}/Apod?date={date}")
    if response.status_code != 200:
        bot.send_message(message.chat.id, "Something went wrong. "
                                          "Make sure your date is past 16.06.1995, the first day an APOD was posted.")
        return

    data = response.json()
    bot.send_photo(message.chat.id, data['url'], caption=data['title'])


@bot.message_handler(commands=['moonphase'])
def get_moon_phase(message):
    args = message.text.split()[1:]
    date = datetime.now().strftime('%Y-%m-%d') if not args else parse_date(args[0], message)
    if not date:
        return

    response = requests.get(f"{API_URL}/MoonPhase?date={date}")
    if response.status_code != 200:
        bot.send_message(message.chat.id, ERROR_MESSAGES["generic"])
        return
    bot.send_photo(message.chat.id, response.text)


@bot.message_handler(commands=['weather'])
def get_weather(message):
    coordinates = get_user_coordinates(message.from_user.id)
    if not coordinates:
        bot.send_message(message.chat.id, ERROR_MESSAGES["coordinates_not_set"])
        return

    latitude, longitude = coordinates
    response = requests.get(f"{API_URL}/Weather?lat={latitude}&lon={longitude}")
    if response.status_code != 200:
        bot.send_message(message.chat.id, "Could not retrieve weather information.")
        return

    bot.send_photo(message.chat.id, BytesIO(response.content), "Current weather at your location:")


@bot.message_handler(commands=['sunspots'])
def get_sunspots(message):
    response = requests.get(f"{API_URL}/Sunspots")
    if response.status_code != 200:
        bot.send_message(message.chat.id, "Could not retrieve sunspots information.")
        return

    bot.send_photo(message.chat.id, BytesIO(response.content), "Current sunspots:")


@bot.message_handler(commands=['graph'])
def get_graph(message):
    args = message.text.split()[1:]
    if not args:
        bot.send_message(message.chat.id, "Please, specify the planet to get a graph representation of its altitude.\n\n"
                                          "You can also add 'extended' to your request to get a graph for 6 months and/or a date.\n\n"
                                          "For example: /graph saturn, /graph jupiter extended, /graph mars extended 24.05.2019, /graph venus 01.06.2024.")
        return

    planet = args[0]
    date = datetime.now().strftime('%Y-%m-%d')
    extended = 'extended' in args
    if len(args) > 2 and extended:
        date = parse_date(args[2], message)
        if not date:
            return

    coordinates = get_user_coordinates(message.from_user.id)
    if not coordinates:
        bot.send_message(message.chat.id, ERROR_MESSAGES["coordinates_not_set"])
        return

    latitude, longitude = coordinates
    time = datetime.now().strftime('%H:%M:%S')

    response = requests.get(f"{API_URL}/SolarSysObjects/graph", params={
        'lat': latitude,
        'lon': longitude,
        'elevation': 200,
        'date': date,
        'time': time,
        'body': planet,
        'extended': extended
    })

    if response.status_code != 200:
        bot.send_message(message.chat.id, ERROR_MESSAGES["generic"])
        return

    bot.send_photo(message.chat.id, BytesIO(response.content), "Graph:")


@bot.message_handler(commands=['planets'])
def get_planets(message):
    args = message.text.split()[1:]
    if not args:
        bot.send_message(message.chat.id, "Please, specify your request.\n\n"
                                          "You can get overall information about visible planets right now and tonight by entering /planets overview.\n\n"
                                          "Also you can enter the planet name to get more detailed information about it.\n\n"
                                          "Optionally add date to your request.\n\nExample: /planets mars, /planets overview 01.06.2024")
        return

    planet = args[0].lower()
    date = datetime.now().strftime('%Y-%m-%d')
    if len(args) > 1:
        date = parse_date(args[1], message)
        if not date:
            return

    coordinates = get_user_coordinates(message.from_user.id)
    if not coordinates:
        bot.send_message(message.chat.id, ERROR_MESSAGES["coordinates_not_set"])
        return

    latitude, longitude = coordinates
    time = datetime.now().strftime('%H:%M:%S')

    if planet == 'overview':
        get_overview(message, latitude, longitude, date, time)
    else:
        get_planet_details(message, planet, latitude, longitude, date, time)


def get_overview(message, latitude, longitude, date, time):
    planets = ['mercury', 'venus', 'mars', 'jupiter', 'saturn', 'uranus', 'neptune', 'pluto']
    visible_planets, visible_planets_tonight = [], []

    for planet in planets:
        response = requests.get(f"{API_URL}/SolarSysObjects", params={
            'lat': latitude,
            'lon': longitude,
            'elev': 200,
            'date': date,
            'time': time,
            'body': planet
        })

        if response.status_code != 200:
            bot.send_message(message.chat.id, ERROR_MESSAGES["generic"])
            return

        data = response.json()
        if data['isVisible']:
            visible_planets.append(planet.capitalize())
        if data['highestAltitudeDegrees'] > 0:
            visible_planets_tonight.append(planet.capitalize())

    visible_planets = ', '.join(visible_planets)
    if len(visible_planets) == 0:
        visible_planets = '❌ None'

    bot.send_message(message.chat.id, f"🪐 Currently visible planets:\n"
                                      f"{visible_planets}\n\n"
                                      f"🌃 Planets visible tonight from 22:00 to 06:00:\n{', '.join(visible_planets_tonight)}")


def get_planet_details(message, planet, latitude, longitude, date, time):
    response = requests.get(f"{API_URL}/SolarSysObjects", params={
        'lat': latitude,
        'lon': longitude,
        'elev': 200,
        'date': date,
        'time': time,
        'body': planet
    })

    if response.status_code != 200:
        bot.send_message(message.chat.id, ERROR_MESSAGES["generic"])
        return

    data = response.json()
    highest_altitude_info = "Not visible tonight" if data['highestAltitudeTime'] is None else f"{data['highestAltitude']} at {data['highestAltitudeTime']}"
    details = [
        f"{data['name'].capitalize()}\n",
        f"🔭 Visible right now: {'✅' if data['isVisible'] else '❌'}",
        f"📐 Current altitude: {data['altitude']}",
        f"📏 Current azimuth: {data['azimuth']}\n",
        f"💫 Constellation: {data['constellation']}",
        f"🔆 Magnitude: {data['magnitude']}\n",
        f"📈 Highest altitude tonight: {highest_altitude_info}"
    ]
    bot.send_message(message.chat.id, "\n".join(details))


def parse_date(date_str, message):
    try:
        return datetime.strptime(date_str, '%d.%m.%Y').strftime('%Y-%m-%d')
    except ValueError:
        bot.send_message(message.chat.id, ERROR_MESSAGES["invalid_date_format"])
        return None


bot.polling()
