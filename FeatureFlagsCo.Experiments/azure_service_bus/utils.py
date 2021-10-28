
def check_format(input={}, key=None, fmt=None, values_range=[]):
    value = input.get(key, None)
    is_valid = False
    if value :
        if type(value) is fmt :
            if values_range:
                if value in values_range :
                    is_valid = True
            else:
                is_valid = True
    return is_valid
