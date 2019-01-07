#include "string_container.h"

#include <list>

std::list<std::string> strings;

std::string& string_database_add(const char* str) {
    strings.emplace_front(str);
    return strings.front();
}