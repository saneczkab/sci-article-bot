from redis_om import Migrator

def set_up_models():
    Migrator().run()